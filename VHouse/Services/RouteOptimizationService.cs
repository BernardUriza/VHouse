using Microsoft.EntityFrameworkCore;
using VHouse.Classes;
using VHouse.Interfaces;

namespace VHouse.Services
{
    /// <summary>
    /// Service for advanced route optimization and logistics management.
    /// </summary>
    public class RouteOptimizationService : IRouteOptimizationService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<RouteOptimizationService> _logger;
        private readonly IDistributionCenterService _distributionCenterService;

        public RouteOptimizationService(
            ApplicationDbContext context, 
            ILogger<RouteOptimizationService> logger,
            IDistributionCenterService distributionCenterService)
        {
            _context = context;
            _logger = logger;
            _distributionCenterService = distributionCenterService;
        }

        /// <summary>
        /// Optimizes delivery routes for a distribution center considering multiple constraints.
        /// </summary>
        public async Task<OptimizedRouteResult> OptimizeRoutesAsync(int distributionCenterId, List<Delivery> deliveries, RouteOptimizationOptions options)
        {
            try
            {
                _logger.LogInformation("Starting route optimization for distribution center {DistributionCenterId} with {DeliveryCount} deliveries", 
                    distributionCenterId, deliveries.Count);

                var distributionCenter = await _distributionCenterService.GetDistributionCenterByIdAsync(distributionCenterId, includeRoutes: true);
                
                if (distributionCenter == null)
                {
                    throw new ArgumentException($"Distribution center {distributionCenterId} not found");
                }

                // Group deliveries by proximity and time windows
                var clusteredDeliveries = await ClusterDeliveriesByProximityAsync(deliveries, options);
                
                var optimizedRoutes = new List<OptimizedRoute>();
                decimal totalDistance = 0;
                TimeSpan totalTime = TimeSpan.Zero;

                foreach (var cluster in clusteredDeliveries)
                {
                    var optimizedSequence = await OptimizeDeliverySequenceInternalAsync(cluster, options);
                    var routeMetrics = await CalculateRouteMetricsAsync(distributionCenter, optimizedSequence);

                    var optimizedRoute = new OptimizedRoute
                    {
                        RouteId = GetNextRouteId(),
                        RouteName = $"Route-{optimizedRoutes.Count + 1}",
                        OptimizedDeliveries = optimizedSequence,
                        RouteDistance = routeMetrics.Distance,
                        EstimatedDuration = routeMetrics.Duration,
                        DeliveryCount = optimizedSequence.Count,
                        UtilizationPercentage = CalculateUtilization(optimizedSequence, options.MaxDeliveriesPerRoute)
                    };

                    optimizedRoutes.Add(optimizedRoute);
                    totalDistance += routeMetrics.Distance;
                    totalTime = totalTime.Add(routeMetrics.Duration);
                }

                var result = new OptimizedRouteResult
                {
                    Routes = optimizedRoutes,
                    TotalDeliveries = deliveries.Count,
                    TotalDistance = Math.Round(totalDistance, 2),
                    TotalTime = totalTime,
                    EstimatedFuelCost = CalculateFuelCost(totalDistance),
                    ImprovementPercentage = await CalculateImprovementPercentageAsync(distributionCenterId, totalDistance, totalTime),
                    OptimizationSummary = GenerateOptimizationSummary(optimizedRoutes, options)
                };

                _logger.LogInformation("Route optimization completed for distribution center {DistributionCenterId}. Created {RouteCount} routes covering {Distance}km", 
                    distributionCenterId, optimizedRoutes.Count, result.TotalDistance);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error optimizing routes for distribution center {DistributionCenterId}", distributionCenterId);
                throw;
            }
        }

        /// <summary>
        /// Calculates optimal route sequence for a specific delivery route.
        /// </summary>
        public async Task<List<Delivery>> OptimizeDeliverySequenceAsync(int routeId, List<Delivery> deliveries)
        {
            try
            {
                var route = await _context.DeliveryRoutes
                    .AsNoTracking()
                    .Include(dr => dr.DistributionCenter)
                    .FirstOrDefaultAsync(dr => dr.DeliveryRouteId == routeId);

                if (route == null)
                {
                    throw new ArgumentException($"Route {routeId} not found");
                }

                var optimizedSequence = await OptimizeDeliverySequenceInternalAsync(deliveries, new RouteOptimizationOptions());

                _logger.LogInformation("Optimized delivery sequence for route {RouteId} with {DeliveryCount} deliveries", 
                    routeId, deliveries.Count);

                return optimizedSequence;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error optimizing delivery sequence for route {RouteId}", routeId);
                throw;
            }
        }

        /// <summary>
        /// Gets estimated travel time between two locations.
        /// </summary>
        public async Task<TimeSpan> GetEstimatedTravelTimeAsync(double fromLatitude, double fromLongitude, double toLatitude, double toLongitude)
        {
            try
            {
                // Calculate distance using Haversine formula
                var distance = CalculateDistance(fromLatitude, fromLongitude, toLatitude, toLongitude);
                
                // Estimate travel time based on average city driving speed (40 km/h)
                var estimatedHours = distance / 40.0;
                var estimatedTime = TimeSpan.FromHours(estimatedHours);

                // Add buffer for traffic and delivery stops (20%)
                var bufferedTime = TimeSpan.FromMilliseconds(estimatedTime.TotalMilliseconds * 1.2);

                _logger.LogDebug("Estimated travel time between ({FromLat}, {FromLon}) and ({ToLat}, {ToLon}): {TravelTime}", 
                    fromLatitude, fromLongitude, toLatitude, toLongitude, bufferedTime);

                return await Task.FromResult(bufferedTime);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating travel time between locations");
                throw;
            }
        }

        /// <summary>
        /// Gets real-time traffic conditions for a route.
        /// </summary>
        public async Task<TrafficConditions> GetTrafficConditionsAsync(List<DeliveryLocation> locations)
        {
            try
            {
                // Simulate traffic conditions (in real implementation, this would call a traffic API)
                var random = new Random();
                var trafficLevel = (TrafficLevel)random.Next(0, 4);
                
                var trafficSegments = new List<TrafficSegment>();
                
                for (int i = 0; i < locations.Count - 1; i++)
                {
                    var segment = new TrafficSegment
                    {
                        FromLatitude = locations[i].Latitude,
                        FromLongitude = locations[i].Longitude,
                        ToLatitude = locations[i + 1].Latitude,
                        ToLongitude = locations[i + 1].Longitude,
                        TrafficLevel = (TrafficLevel)random.Next(0, 4),
                        EstimatedDelay = TimeSpan.FromMinutes(random.Next(0, 15)),
                        Description = GetTrafficDescription((TrafficLevel)random.Next(0, 4))
                    };
                    
                    trafficSegments.Add(segment);
                }

                var conditions = new TrafficConditions
                {
                    OverallTrafficLevel = trafficLevel,
                    TrafficSegments = trafficSegments,
                    EstimatedDelay = TimeSpan.FromMinutes(trafficSegments.Sum(s => s.EstimatedDelay.TotalMinutes)),
                    Incidents = GetTrafficIncidents(trafficLevel),
                    LastUpdated = DateTime.UtcNow
                };

                _logger.LogInformation("Retrieved traffic conditions for {SegmentCount} segments. Overall level: {TrafficLevel}", 
                    trafficSegments.Count, trafficLevel);

                return await Task.FromResult(conditions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving traffic conditions");
                throw;
            }
        }

        /// <summary>
        /// Suggests alternative routes in case of traffic or delivery issues.
        /// </summary>
        public async Task<List<AlternativeRoute>> GetAlternativeRoutesAsync(int routeId, RouteIssue issue)
        {
            try
            {
                var route = await _context.DeliveryRoutes
                    .AsNoTracking()
                    .Include(dr => dr.Deliveries)
                    .FirstOrDefaultAsync(dr => dr.DeliveryRouteId == routeId);

                if (route == null)
                {
                    throw new ArgumentException($"Route {routeId} not found");
                }

                var alternatives = new List<AlternativeRoute>();

                // Generate alternative routes based on issue type
                switch (issue.Type)
                {
                    case RouteIssueType.TrafficJam:
                    case RouteIssueType.RoadClosure:
                        alternatives.AddRange(await GenerateDetourAlternativesAsync(route, issue));
                        break;
                    case RouteIssueType.VehicleBreakdown:
                        alternatives.AddRange(await GenerateBackupVehicleAlternativesAsync(route, issue));
                        break;
                    case RouteIssueType.DeliveryFailure:
                        alternatives.AddRange(await GenerateDeliveryRescheduleAlternativesAsync(route, issue));
                        break;
                    default:
                        alternatives.AddRange(await GenerateGenericAlternativesAsync(route, issue));
                        break;
                }

                _logger.LogInformation("Generated {AlternativeCount} alternative routes for route {RouteId} due to {IssueType}", 
                    alternatives.Count, routeId, issue.Type);

                return alternatives;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating alternative routes for route {RouteId}", routeId);
                throw;
            }
        }

        /// <summary>
        /// Validates route feasibility considering time windows and vehicle capacity.
        /// </summary>
        public async Task<RouteValidationResult> ValidateRouteAsync(DeliveryRoute route, List<Delivery> deliveries)
        {
            try
            {
                var validationResult = new RouteValidationResult
                {
                    IsValid = true,
                    Issues = new List<ValidationIssue>(),
                    CapacityAnalysis = await AnalyzeRouteCapacityAsync(route, deliveries),
                    TimeWindowAnalysis = await AnalyzeTimeWindowsAsync(deliveries)
                };

                // Validate capacity constraints
                if (validationResult.CapacityAnalysis.IsOverCapacity)
                {
                    validationResult.IsValid = false;
                    validationResult.Issues.Add(new ValidationIssue
                    {
                        Type = ValidationIssueType.CapacityExceeded,
                        Message = $"Route exceeds maximum capacity. Current utilization: {validationResult.CapacityAnalysis.WeightUtilization:P1}",
                        Severity = ValidationSeverity.Error
                    });
                }

                // Validate time windows
                if (validationResult.TimeWindowAnalysis.HasConflicts)
                {
                    validationResult.IsValid = false;
                    validationResult.Issues.Add(new ValidationIssue
                    {
                        Type = ValidationIssueType.TimeWindowViolation,
                        Message = $"Route has {validationResult.TimeWindowAnalysis.TimeWindowViolations} time window violations",
                        Severity = ValidationSeverity.Error
                    });
                }

                // Check for duplicate deliveries
                var duplicateDeliveries = deliveries
                    .GroupBy(d => new { d.DeliveryAddress, d.RecipientName })
                    .Where(g => g.Count() > 1)
                    .ToList();

                foreach (var duplicate in duplicateDeliveries)
                {
                    validationResult.IsValid = false;
                    validationResult.Issues.Add(new ValidationIssue
                    {
                        Type = ValidationIssueType.DuplicateDelivery,
                        Message = $"Duplicate delivery found for {duplicate.Key.RecipientName} at {duplicate.Key.DeliveryAddress}",
                        Severity = ValidationSeverity.Warning
                    });
                }

                validationResult.Summary = GenerateValidationSummary(validationResult);

                _logger.LogInformation("Route validation completed for route {RouteId}. Valid: {IsValid}, Issues: {IssueCount}", 
                    route.DeliveryRouteId, validationResult.IsValid, validationResult.Issues.Count);

                return validationResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating route {RouteId}", route.DeliveryRouteId);
                throw;
            }
        }

        /// <summary>
        /// Gets route performance analytics and KPIs.
        /// </summary>
        public async Task<RoutePerformanceMetrics> GetRoutePerformanceAsync(int routeId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                var route = await _context.DeliveryRoutes
                    .AsNoTracking()
                    .FirstOrDefaultAsync(dr => dr.DeliveryRouteId == routeId);

                if (route == null)
                {
                    throw new ArgumentException($"Route {routeId} not found");
                }

                fromDate ??= DateTime.UtcNow.AddDays(-30);
                toDate ??= DateTime.UtcNow;

                var deliveries = await _context.Deliveries
                    .AsNoTracking()
                    .Where(d => d.DeliveryRouteId == routeId &&
                               d.ScheduledDate >= fromDate && d.ScheduledDate <= toDate)
                    .ToListAsync();

                var successfulDeliveries = deliveries.Count(d => d.Status == "Delivered");
                var failedDeliveries = deliveries.Count(d => d.Status == "Failed");

                var averageDeliveryTime = deliveries
                    .Where(d => d.ActualDeliveryDate.HasValue)
                    .Select(d => (d.ActualDeliveryDate!.Value - d.ScheduledDate).TotalMinutes)
                    .DefaultIfEmpty(0)
                    .Average();

                var metrics = new RoutePerformanceMetrics
                {
                    RouteId = routeId,
                    RouteName = route.Name,
                    TotalDeliveries = deliveries.Count,
                    SuccessfulDeliveries = successfulDeliveries,
                    FailedDeliveries = failedDeliveries,
                    SuccessRate = deliveries.Count > 0 ? (decimal)successfulDeliveries / deliveries.Count * 100 : 0,
                    AverageDeliveryTime = TimeSpan.FromMinutes(averageDeliveryTime),
                    AverageDistance = route.Distance,
                    FuelConsumption = CalculateFuelConsumption(route.Distance, deliveries.Count),
                    OperatingCost = CalculateOperatingCost(route.Distance, deliveries.Count),
                    CustomerSatisfactionScore = CalculateCustomerSatisfaction(successfulDeliveries, failedDeliveries),
                    PeriodStart = fromDate.Value,
                    PeriodEnd = toDate.Value
                };

                _logger.LogInformation("Retrieved performance metrics for route {RouteId} from {FromDate} to {ToDate}", 
                    routeId, fromDate, toDate);

                return metrics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving performance metrics for route {RouteId}", routeId);
                throw;
            }
        }

        #region Private Helper Methods

        private async Task<List<List<Delivery>>> ClusterDeliveriesByProximityAsync(List<Delivery> deliveries, RouteOptimizationOptions options)
        {
            var clusters = new List<List<Delivery>>();
            var unassignedDeliveries = new List<Delivery>(deliveries);

            while (unassignedDeliveries.Any())
            {
                var cluster = new List<Delivery>();
                var seedDelivery = unassignedDeliveries.First();
                cluster.Add(seedDelivery);
                unassignedDeliveries.Remove(seedDelivery);

                // Find nearby deliveries
                var nearbyDeliveries = unassignedDeliveries
                    .Where(d => CalculateDistance(
                        seedDelivery.Latitude ?? 0, seedDelivery.Longitude ?? 0,
                        d.Latitude ?? 0, d.Longitude ?? 0) <= 10) // Within 10km
                    .Take(options.MaxDeliveriesPerRoute - 1)
                    .ToList();

                cluster.AddRange(nearbyDeliveries);
                unassignedDeliveries.RemoveAll(d => nearbyDeliveries.Contains(d));
                
                clusters.Add(cluster);
            }

            return await Task.FromResult(clusters);
        }

        private async Task<List<Delivery>> OptimizeDeliverySequenceInternalAsync(List<Delivery> deliveries, RouteOptimizationOptions options)
        {
            // Simple nearest neighbor algorithm for sequence optimization
            if (deliveries.Count <= 1)
                return deliveries;

            var optimizedSequence = new List<Delivery>();
            var remaining = new List<Delivery>(deliveries);

            // Start with delivery closest to distribution center (assuming 0,0 for simplicity)
            var current = remaining.OrderBy(d => 
                CalculateDistance(0, 0, d.Latitude ?? 0, d.Longitude ?? 0)).First();
            
            optimizedSequence.Add(current);
            remaining.Remove(current);

            while (remaining.Any())
            {
                var next = remaining.OrderBy(d => 
                    CalculateDistance(
                        current.Latitude ?? 0, current.Longitude ?? 0,
                        d.Latitude ?? 0, d.Longitude ?? 0)).First();
                
                optimizedSequence.Add(next);
                remaining.Remove(next);
                current = next;
            }

            return await Task.FromResult(optimizedSequence);
        }

        private async Task<(decimal Distance, TimeSpan Duration)> CalculateRouteMetricsAsync(DistributionCenter center, List<Delivery> deliveries)
        {
            decimal totalDistance = 0;
            var totalDuration = TimeSpan.Zero;

            if (!deliveries.Any())
                return (0, TimeSpan.Zero);

            // Distance from center to first delivery
            if (center.Latitude.HasValue && center.Longitude.HasValue && deliveries.First().Latitude.HasValue)
            {
                totalDistance += (decimal)CalculateDistance(
                    center.Latitude.Value, center.Longitude.Value,
                    deliveries.First().Latitude!.Value, deliveries.First().Longitude!.Value);
            }

            // Distance between consecutive deliveries
            for (int i = 0; i < deliveries.Count - 1; i++)
            {
                if (deliveries[i].Latitude.HasValue && deliveries[i + 1].Latitude.HasValue)
                {
                    totalDistance += (decimal)CalculateDistance(
                        deliveries[i].Latitude!.Value, deliveries[i].Longitude!.Value,
                        deliveries[i + 1].Latitude!.Value, deliveries[i + 1].Longitude!.Value);
                }
            }

            // Distance from last delivery back to center
            if (center.Latitude.HasValue && center.Longitude.HasValue && deliveries.Last().Latitude.HasValue)
            {
                totalDistance += (decimal)CalculateDistance(
                    deliveries.Last().Latitude!.Value, deliveries.Last().Longitude!.Value,
                    center.Latitude.Value, center.Longitude.Value);
            }

            // Estimate duration (40 km/h average speed + 15 minutes per delivery)
            var travelTime = TimeSpan.FromHours((double)totalDistance / 40.0);
            var deliveryTime = TimeSpan.FromMinutes(deliveries.Count * 15);
            totalDuration = travelTime.Add(deliveryTime);

            return await Task.FromResult((Math.Round(totalDistance, 2), totalDuration));
        }

        private static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371; // Earth's radius in kilometers

            var lat1Rad = lat1 * Math.PI / 180;
            var lat2Rad = lat2 * Math.PI / 180;
            var deltaLatRad = (lat2 - lat1) * Math.PI / 180;
            var deltaLonRad = (lon2 - lon1) * Math.PI / 180;

            var a = Math.Sin(deltaLatRad / 2) * Math.Sin(deltaLatRad / 2) +
                    Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
                    Math.Sin(deltaLonRad / 2) * Math.Sin(deltaLonRad / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return R * c;
        }

        private static decimal CalculateUtilization(List<Delivery> deliveries, int maxCapacity) =>
            maxCapacity > 0 ? (decimal)deliveries.Count / maxCapacity * 100 : 0;

        private static decimal CalculateFuelCost(decimal distance) =>
            distance * 0.15m; // $0.15 per km

        private static decimal CalculateFuelConsumption(decimal distance, int deliveries) =>
            (distance * 0.08m) + (deliveries * 0.5m); // Base consumption + delivery stops

        private static decimal CalculateOperatingCost(decimal distance, int deliveries) =>
            (distance * 0.25m) + (deliveries * 2.0m); // Fuel + driver time + vehicle wear

        private static decimal CalculateCustomerSatisfaction(int successful, int failed) =>
            successful + failed > 0 ? (decimal)successful / (successful + failed) * 100 : 0;

        private async Task<decimal> CalculateImprovementPercentageAsync(int distributionCenterId, decimal newDistance, TimeSpan newTime)
        {
            // Compare with historical average (simplified)
            var historicalAverage = await _context.Deliveries
                .AsNoTracking()
                .Where(d => d.DeliveryRoute!.DistributionCenterId == distributionCenterId)
                .Select(d => d.DeliveryRoute!.Distance)
                .DefaultIfEmpty(0)
                .AverageAsync();

            if (historicalAverage > 0)
            {
                return Math.Round(((decimal)historicalAverage - newDistance) / (decimal)historicalAverage * 100, 2);
            }

            return 0;
        }

        private static string GenerateOptimizationSummary(List<OptimizedRoute> routes, RouteOptimizationOptions options) =>
            $"Optimized {routes.Count} routes with {options.Priority} priority. " +
            $"Total deliveries: {routes.Sum(r => r.DeliveryCount)}, " +
            $"Average utilization: {routes.Average(r => r.UtilizationPercentage):F1}%";

        private static string GetTrafficDescription(TrafficLevel level) => level switch
        {
            TrafficLevel.Light => "Light traffic, normal conditions",
            TrafficLevel.Moderate => "Moderate traffic, slight delays expected",
            TrafficLevel.Heavy => "Heavy traffic, significant delays",
            TrafficLevel.Severe => "Severe traffic, major delays and congestion",
            _ => "Unknown traffic conditions"
        };

        private static string[] GetTrafficIncidents(TrafficLevel level) => level switch
        {
            TrafficLevel.Light => Array.Empty<string>(),
            TrafficLevel.Moderate => new[] { "Minor construction work" },
            TrafficLevel.Heavy => new[] { "Road construction", "Minor accident" },
            TrafficLevel.Severe => new[] { "Major accident", "Road closure", "Weather conditions" },
            _ => Array.Empty<string>()
        };

        private static int GetNextRouteId() => Random.Shared.Next(10000, 99999);

        private async Task<List<AlternativeRoute>> GenerateDetourAlternativesAsync(DeliveryRoute route, RouteIssue issue) =>
            await Task.FromResult(new List<AlternativeRoute>
            {
                new AlternativeRoute
                {
                    RouteId = GetNextRouteId(),
                    Description = "Detour around traffic/closure",
                    AdditionalDistance = 5.5m,
                    AdditionalTime = TimeSpan.FromMinutes(15),
                    CostImpact = 2.75m,
                    AffectedDeliveries = route.Deliveries.Take(3).ToList(),
                    Reason = $"Avoiding {issue.Type}"
                }
            });

        private async Task<List<AlternativeRoute>> GenerateBackupVehicleAlternativesAsync(DeliveryRoute route, RouteIssue issue) =>
            await Task.FromResult(new List<AlternativeRoute>
            {
                new AlternativeRoute
                {
                    RouteId = GetNextRouteId(),
                    Description = "Transfer to backup vehicle",
                    AdditionalDistance = 0,
                    AdditionalTime = TimeSpan.FromMinutes(30),
                    CostImpact = 15.0m,
                    AffectedDeliveries = route.Deliveries.ToList(),
                    Reason = "Vehicle breakdown response"
                }
            });

        private async Task<List<AlternativeRoute>> GenerateDeliveryRescheduleAlternativesAsync(DeliveryRoute route, RouteIssue issue) =>
            await Task.FromResult(new List<AlternativeRoute>
            {
                new AlternativeRoute
                {
                    RouteId = GetNextRouteId(),
                    Description = "Reschedule failed delivery",
                    AdditionalDistance = 2.0m,
                    AdditionalTime = TimeSpan.FromMinutes(20),
                    CostImpact = 5.0m,
                    AffectedDeliveries = route.Deliveries.Take(1).ToList(),
                    Reason = "Delivery failure recovery"
                }
            });

        private async Task<List<AlternativeRoute>> GenerateGenericAlternativesAsync(DeliveryRoute route, RouteIssue issue) =>
            await Task.FromResult(new List<AlternativeRoute>
            {
                new AlternativeRoute
                {
                    RouteId = GetNextRouteId(),
                    Description = "Alternative route plan",
                    AdditionalDistance = 3.0m,
                    AdditionalTime = TimeSpan.FromMinutes(10),
                    CostImpact = 4.0m,
                    AffectedDeliveries = route.Deliveries.Take(2).ToList(),
                    Reason = $"Issue mitigation: {issue.Type}"
                }
            });

        private async Task<RouteCapacityAnalysis> AnalyzeRouteCapacityAsync(DeliveryRoute route, List<Delivery> deliveries)
        {
            // Simplified capacity analysis (would need order item weights/volumes in real implementation)
            var totalItems = deliveries.Count;
            var estimatedWeight = totalItems * 5.0m; // 5kg average per delivery
            var estimatedVolume = totalItems * 0.02m; // 0.02 cubic meters per delivery

            return await Task.FromResult(new RouteCapacityAnalysis
            {
                TotalWeight = estimatedWeight,
                TotalVolume = estimatedVolume,
                TotalItems = totalItems,
                WeightUtilization = route.MaxCapacity > 0 ? estimatedWeight / route.MaxCapacity * 100 : 0,
                VolumeUtilization = 75.0m, // Simplified
                IsOverCapacity = totalItems > route.MaxCapacity
            });
        }

        private async Task<TimeWindowAnalysis> AnalyzeTimeWindowsAsync(List<Delivery> deliveries)
        {
            var deliveriesWithWindows = deliveries.Where(d => d.ScheduledDate != default).ToList();
            
            return await Task.FromResult(new TimeWindowAnalysis
            {
                TotalDeliveries = deliveries.Count,
                DeliveriesWithTimeWindows = deliveriesWithWindows.Count,
                TimeWindowViolations = 0, // Simplified
                EarliestStart = deliveriesWithWindows.Any() ? TimeSpan.FromHours(deliveriesWithWindows.Min(d => d.ScheduledDate.Hour)) : TimeSpan.Zero,
                LatestFinish = deliveriesWithWindows.Any() ? TimeSpan.FromHours(deliveriesWithWindows.Max(d => d.ScheduledDate.Hour)) : TimeSpan.Zero,
                HasConflicts = false // Simplified
            });
        }

        private static string GenerateValidationSummary(RouteValidationResult result)
        {
            if (result.IsValid)
            {
                return "Route validation passed successfully";
            }

            var errorCount = result.Issues.Count(i => i.Severity == ValidationSeverity.Error);
            var warningCount = result.Issues.Count(i => i.Severity == ValidationSeverity.Warning);
            
            return $"Route validation failed: {errorCount} errors, {warningCount} warnings";
        }

        #endregion
    }
}