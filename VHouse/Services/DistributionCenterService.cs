using Microsoft.EntityFrameworkCore;
using VHouse.Classes;
using VHouse.Interfaces;
using VHouse.Extensions;

namespace VHouse.Services
{
    /// <summary>
    /// Service for distribution center management operations.
    /// </summary>
    public class DistributionCenterService : IDistributionCenterService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DistributionCenterService> _logger;

        public DistributionCenterService(ApplicationDbContext context, ILogger<DistributionCenterService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Gets all distribution centers for a tenant with pagination.
        /// </summary>
        public async Task<PagedResult<DistributionCenter>> GetDistributionCentersByTenantAsync(int tenantId, int page = 1, int pageSize = 20)
        {
            try
            {
                var query = _context.DistributionCenters
                    .AsNoTracking()
                    .Where(dc => dc.TenantId == tenantId && dc.IsActive)
                    .OrderBy(dc => dc.Name);

                var pagedResult = await query.ToPagedResultAsync(page, pageSize);
                
                _logger.LogInformation("Retrieved {Count} distribution centers for tenant {TenantId}, page {Page}", 
                    pagedResult.Items.Count, tenantId, page);
                
                return pagedResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving distribution centers for tenant {TenantId}, page {Page}", tenantId, page);
                throw;
            }
        }

        /// <summary>
        /// Gets active distribution centers within a geographic radius.
        /// </summary>
        public async Task<List<DistributionCenter>> GetDistributionCentersInRadiusAsync(double latitude, double longitude, double radiusKm)
        {
            try
            {
                // Using Haversine formula approximation for distance calculation
                var distributionCenters = await _context.DistributionCenters
                    .AsNoTracking()
                    .Where(dc => dc.IsActive && 
                                dc.Latitude.HasValue && 
                                dc.Longitude.HasValue)
                    .ToListAsync();

                var centersInRadius = distributionCenters
                    .Where(dc => CalculateDistance(latitude, longitude, dc.Latitude!.Value, dc.Longitude!.Value) <= radiusKm)
                    .OrderBy(dc => CalculateDistance(latitude, longitude, dc.Latitude!.Value, dc.Longitude!.Value))
                    .ToList();

                _logger.LogInformation("Found {Count} distribution centers within {RadiusKm}km of coordinates ({Latitude}, {Longitude})", 
                    centersInRadius.Count, radiusKm, latitude, longitude);

                return centersInRadius;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving distribution centers in radius {RadiusKm}km", radiusKm);
                throw;
            }
        }

        /// <summary>
        /// Gets a distribution center by ID including related data.
        /// </summary>
        public async Task<DistributionCenter?> GetDistributionCenterByIdAsync(int distributionCenterId, bool includeWarehouses = false, bool includeRoutes = false)
        {
            try
            {
                var query = _context.DistributionCenters.AsNoTracking();

                if (includeWarehouses)
                {
                    query = query.Include(dc => dc.Warehouses.Where(w => w.IsActive));
                }

                if (includeRoutes)
                {
                    query = query.Include(dc => dc.DeliveryRoutes.Where(dr => dr.IsActive));
                }

                var distributionCenter = await query.FirstOrDefaultAsync(dc => dc.DistributionCenterId == distributionCenterId);

                if (distributionCenter != null)
                {
                    _logger.LogInformation("Retrieved distribution center {DistributionCenterId} - {CenterName}", 
                        distributionCenter.DistributionCenterId, distributionCenter.Name);
                }
                else
                {
                    _logger.LogWarning("Distribution center with ID {DistributionCenterId} not found", distributionCenterId);
                }

                return distributionCenter;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving distribution center with ID {DistributionCenterId}", distributionCenterId);
                throw;
            }
        }

        /// <summary>
        /// Creates a new distribution center.
        /// </summary>
        public async Task<DistributionCenter> CreateDistributionCenterAsync(DistributionCenter distributionCenter)
        {
            try
            {
                // Check for unique center code within tenant
                var existingCenter = await _context.DistributionCenters
                    .AsNoTracking()
                    .FirstOrDefaultAsync(dc => dc.TenantId == distributionCenter.TenantId && 
                                             dc.CenterCode == distributionCenter.CenterCode);

                if (existingCenter != null)
                {
                    throw new InvalidOperationException($"Distribution center with code {distributionCenter.CenterCode} already exists for this tenant");
                }

                _context.DistributionCenters.Add(distributionCenter);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created new distribution center {CenterCode} - {CenterName} with ID {DistributionCenterId}", 
                    distributionCenter.CenterCode, distributionCenter.Name, distributionCenter.DistributionCenterId);

                return distributionCenter;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating distribution center {CenterCode}", distributionCenter.CenterCode);
                throw;
            }
        }

        /// <summary>
        /// Updates an existing distribution center.
        /// </summary>
        public async Task<bool> UpdateDistributionCenterAsync(DistributionCenter distributionCenter)
        {
            try
            {
                var existingCenter = await _context.DistributionCenters
                    .FirstOrDefaultAsync(dc => dc.DistributionCenterId == distributionCenter.DistributionCenterId);

                if (existingCenter == null)
                {
                    _logger.LogWarning("Distribution center with ID {DistributionCenterId} not found for update", distributionCenter.DistributionCenterId);
                    return false;
                }

                // Check for unique center code within tenant (excluding current center)
                var duplicateCode = await _context.DistributionCenters
                    .AsNoTracking()
                    .AnyAsync(dc => dc.TenantId == distributionCenter.TenantId && 
                                   dc.CenterCode == distributionCenter.CenterCode && 
                                   dc.DistributionCenterId != distributionCenter.DistributionCenterId);

                if (duplicateCode)
                {
                    throw new InvalidOperationException($"Distribution center with code {distributionCenter.CenterCode} already exists for this tenant");
                }

                existingCenter.CenterCode = distributionCenter.CenterCode;
                existingCenter.Name = distributionCenter.Name;
                existingCenter.Address = distributionCenter.Address;
                existingCenter.Latitude = distributionCenter.Latitude;
                existingCenter.Longitude = distributionCenter.Longitude;
                existingCenter.ManagerName = distributionCenter.ManagerName;
                existingCenter.Phone = distributionCenter.Phone;
                existingCenter.Email = distributionCenter.Email;
                existingCenter.IsActive = distributionCenter.IsActive;
                existingCenter.Capacity = distributionCenter.Capacity;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated distribution center {CenterCode} - {CenterName}", 
                    distributionCenter.CenterCode, distributionCenter.Name);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating distribution center {DistributionCenterId}", distributionCenter.DistributionCenterId);
                throw;
            }
        }

        /// <summary>
        /// Deactivates a distribution center and all its routes.
        /// </summary>
        public async Task<bool> DeactivateDistributionCenterAsync(int distributionCenterId)
        {
            try
            {
                var distributionCenter = await _context.DistributionCenters
                    .Include(dc => dc.DeliveryRoutes)
                    .FirstOrDefaultAsync(dc => dc.DistributionCenterId == distributionCenterId);

                if (distributionCenter == null)
                {
                    _logger.LogWarning("Distribution center with ID {DistributionCenterId} not found for deactivation", distributionCenterId);
                    return false;
                }

                distributionCenter.IsActive = false;

                // Deactivate all delivery routes
                foreach (var route in distributionCenter.DeliveryRoutes)
                {
                    route.IsActive = false;
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Deactivated distribution center {CenterCode} and all its delivery routes", 
                    distributionCenter.CenterCode);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating distribution center {DistributionCenterId}", distributionCenterId);
                throw;
            }
        }

        /// <summary>
        /// Gets distribution center capacity utilization.
        /// </summary>
        public async Task<DistributionCenterCapacity> GetDistributionCenterCapacityAsync(int distributionCenterId)
        {
            try
            {
                var distributionCenter = await _context.DistributionCenters
                    .AsNoTracking()
                    .FirstOrDefaultAsync(dc => dc.DistributionCenterId == distributionCenterId);

                if (distributionCenter == null)
                {
                    throw new ArgumentException($"Distribution center with ID {distributionCenterId} not found");
                }

                // Calculate current load based on pending/in-transit deliveries
                var currentLoad = await _context.Deliveries
                    .AsNoTracking()
                    .Where(d => d.DeliveryRoute!.DistributionCenterId == distributionCenterId &&
                               (d.Status == "Pending" || d.Status == "InTransit"))
                    .CountAsync();

                var utilizationPercentage = distributionCenter.Capacity > 0 
                    ? (decimal)currentLoad / distributionCenter.Capacity * 100 
                    : 0;

                var capacity = new DistributionCenterCapacity
                {
                    DistributionCenterId = distributionCenterId,
                    CenterName = distributionCenter.Name,
                    MaxCapacity = distributionCenter.Capacity,
                    CurrentLoad = currentLoad,
                    UtilizationPercentage = Math.Round(utilizationPercentage, 2),
                    AvailableCapacity = Math.Max(0, distributionCenter.Capacity - currentLoad),
                    IsOverCapacity = currentLoad > distributionCenter.Capacity
                };

                _logger.LogInformation("Retrieved capacity for distribution center {DistributionCenterId}: {UtilizationPercentage}% utilized", 
                    distributionCenterId, capacity.UtilizationPercentage);

                return capacity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving capacity for distribution center {DistributionCenterId}", distributionCenterId);
                throw;
            }
        }

        /// <summary>
        /// Gets optimal distribution center for a delivery location.
        /// </summary>
        public async Task<DistributionCenter?> GetOptimalDistributionCenterAsync(double deliveryLatitude, double deliveryLongitude, int tenantId)
        {
            try
            {
                var distributionCenters = await _context.DistributionCenters
                    .AsNoTracking()
                    .Where(dc => dc.TenantId == tenantId && 
                                dc.IsActive && 
                                dc.Latitude.HasValue && 
                                dc.Longitude.HasValue)
                    .ToListAsync();

                if (!distributionCenters.Any())
                {
                    _logger.LogWarning("No active distribution centers found for tenant {TenantId}", tenantId);
                    return null;
                }

                // Calculate distances and find optimal center (closest with available capacity)
                var centerDistances = distributionCenters
                    .Select(dc => new 
                    {
                        Center = dc,
                        Distance = CalculateDistance(deliveryLatitude, deliveryLongitude, 
                                                   dc.Latitude!.Value, dc.Longitude!.Value)
                    })
                    .OrderBy(cd => cd.Distance)
                    .ToList();

                // Get capacity information for centers
                foreach (var centerDistance in centerDistances)
                {
                    var capacity = await GetDistributionCenterCapacityAsync(centerDistance.Center.DistributionCenterId);
                    
                    // Return first center with available capacity
                    if (!capacity.IsOverCapacity)
                    {
                        _logger.LogInformation("Selected distribution center {DistributionCenterId} for delivery at ({Latitude}, {Longitude}), distance: {Distance}km", 
                            centerDistance.Center.DistributionCenterId, deliveryLatitude, deliveryLongitude, Math.Round(centerDistance.Distance, 2));
                        
                        return centerDistance.Center;
                    }
                }

                // If all centers are over capacity, return the closest one
                var closestCenter = centerDistances.First().Center;
                
                _logger.LogWarning("All distribution centers are over capacity, returning closest center {DistributionCenterId}", 
                    closestCenter.DistributionCenterId);
                
                return closestCenter;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding optimal distribution center for delivery location ({Latitude}, {Longitude})", 
                    deliveryLatitude, deliveryLongitude);
                throw;
            }
        }

        /// <summary>
        /// Gets distribution center performance metrics.
        /// </summary>
        public async Task<DistributionCenterMetrics> GetDistributionCenterMetricsAsync(int distributionCenterId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                var distributionCenter = await _context.DistributionCenters
                    .AsNoTracking()
                    .FirstOrDefaultAsync(dc => dc.DistributionCenterId == distributionCenterId);

                if (distributionCenter == null)
                {
                    throw new ArgumentException($"Distribution center with ID {distributionCenterId} not found");
                }

                fromDate ??= DateTime.UtcNow.AddDays(-30);
                toDate ??= DateTime.UtcNow;

                var deliveriesQuery = _context.Deliveries
                    .AsNoTracking()
                    .Where(d => d.DeliveryRoute!.DistributionCenterId == distributionCenterId &&
                               d.ScheduledDate >= fromDate && d.ScheduledDate <= toDate);

                var totalDeliveries = await deliveriesQuery.CountAsync();
                var completedDeliveries = await deliveriesQuery.CountAsync(d => d.Status == "Delivered");
                var failedDeliveries = await deliveriesQuery.CountAsync(d => d.Status == "Failed");

                var deliverySuccessRate = totalDeliveries > 0 
                    ? (decimal)completedDeliveries / totalDeliveries * 100 
                    : 0;

                var deliveriesWithTimes = await deliveriesQuery
                    .Where(d => d.Status == "Delivered" && d.ActualDeliveryDate.HasValue)
                    .Select(d => new { d.ScheduledDate, d.ActualDeliveryDate })
                    .ToListAsync();

                var averageDeliveryTime = deliveriesWithTimes.Any() 
                    ? deliveriesWithTimes.Average(d => (d.ActualDeliveryDate!.Value - d.ScheduledDate).TotalMinutes)
                    : 0;

                var activeRoutes = await _context.DeliveryRoutes
                    .AsNoTracking()
                    .CountAsync(dr => dr.DistributionCenterId == distributionCenterId && dr.IsActive);

                var totalWarehouses = await _context.Warehouses
                    .AsNoTracking()
                    .CountAsync(w => w.DistributionCenterId == distributionCenterId);

                var metrics = new DistributionCenterMetrics
                {
                    DistributionCenterId = distributionCenterId,
                    CenterName = distributionCenter.Name,
                    TotalDeliveries = totalDeliveries,
                    CompletedDeliveries = completedDeliveries,
                    FailedDeliveries = failedDeliveries,
                    DeliverySuccessRate = Math.Round(deliverySuccessRate, 2),
                    AverageDeliveryTime = Math.Round((decimal)averageDeliveryTime, 2),
                    ActiveRoutes = activeRoutes,
                    TotalWarehouses = totalWarehouses,
                    PeriodStart = fromDate.Value,
                    PeriodEnd = toDate.Value
                };

                _logger.LogInformation("Retrieved metrics for distribution center {DistributionCenterId} from {FromDate} to {ToDate}", 
                    distributionCenterId, fromDate, toDate);

                return metrics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving metrics for distribution center {DistributionCenterId}", distributionCenterId);
                throw;
            }
        }

        /// <summary>
        /// Calculates the distance between two geographic points using the Haversine formula.
        /// </summary>
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
    }
}