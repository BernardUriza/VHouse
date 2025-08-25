using VHouse.Classes;

namespace VHouse.Interfaces
{
    /// <summary>
    /// Service interface for advanced route optimization and logistics management.
    /// </summary>
    public interface IRouteOptimizationService
    {
        /// <summary>
        /// Optimizes delivery routes for a distribution center considering multiple constraints.
        /// </summary>
        Task<OptimizedRouteResult> OptimizeRoutesAsync(int distributionCenterId, List<Delivery> deliveries, RouteOptimizationOptions options);

        /// <summary>
        /// Calculates optimal route sequence for a specific delivery route.
        /// </summary>
        Task<List<Delivery>> OptimizeDeliverySequenceAsync(int routeId, List<Delivery> deliveries);

        /// <summary>
        /// Gets estimated travel time between two locations.
        /// </summary>
        Task<TimeSpan> GetEstimatedTravelTimeAsync(double fromLatitude, double fromLongitude, double toLatitude, double toLongitude);

        /// <summary>
        /// Gets real-time traffic conditions for a route.
        /// </summary>
        Task<TrafficConditions> GetTrafficConditionsAsync(List<DeliveryLocation> locations);

        /// <summary>
        /// Suggests alternative routes in case of traffic or delivery issues.
        /// </summary>
        Task<List<AlternativeRoute>> GetAlternativeRoutesAsync(int routeId, RouteIssue issue);

        /// <summary>
        /// Validates route feasibility considering time windows and vehicle capacity.
        /// </summary>
        Task<RouteValidationResult> ValidateRouteAsync(DeliveryRoute route, List<Delivery> deliveries);

        /// <summary>
        /// Gets route performance analytics and KPIs.
        /// </summary>
        Task<RoutePerformanceMetrics> GetRoutePerformanceAsync(int routeId, DateTime? fromDate = null, DateTime? toDate = null);
    }

    /// <summary>
    /// Options for route optimization algorithms.
    /// </summary>
    public class RouteOptimizationOptions
    {
        /// <summary>
        /// Optimization priority: Distance, Time, Cost, or Balanced.
        /// </summary>
        public OptimizationPriority Priority { get; set; } = OptimizationPriority.Balanced;

        /// <summary>
        /// Consider real-time traffic data.
        /// </summary>
        public bool UseRealTimeTraffic { get; set; } = true;

        /// <summary>
        /// Consider delivery time windows.
        /// </summary>
        public bool RespectTimeWindows { get; set; } = true;

        /// <summary>
        /// Maximum route duration in hours.
        /// </summary>
        public int MaxRouteDurationHours { get; set; } = 8;

        /// <summary>
        /// Maximum number of deliveries per route.
        /// </summary>
        public int MaxDeliveriesPerRoute { get; set; } = 50;

        /// <summary>
        /// Vehicle capacity constraints.
        /// </summary>
        public VehicleCapacityConstraints? CapacityConstraints { get; set; }
    }

    /// <summary>
    /// Optimization priority enumeration.
    /// </summary>
    public enum OptimizationPriority
    {
        Distance,
        Time,
        Cost,
        Balanced
    }

    /// <summary>
    /// Vehicle capacity constraints.
    /// </summary>
    public class VehicleCapacityConstraints
    {
        public decimal MaxWeight { get; set; }
        public decimal MaxVolume { get; set; }
        public int MaxItems { get; set; }
    }

    /// <summary>
    /// Result of route optimization.
    /// </summary>
    public class OptimizedRouteResult
    {
        public List<OptimizedRoute> Routes { get; set; } = new();
        public int TotalDeliveries { get; set; }
        public decimal TotalDistance { get; set; }
        public TimeSpan TotalTime { get; set; }
        public decimal EstimatedFuelCost { get; set; }
        public decimal ImprovementPercentage { get; set; }
        public string OptimizationSummary { get; set; } = string.Empty;
    }

    /// <summary>
    /// Optimized route information.
    /// </summary>
    public class OptimizedRoute
    {
        public int RouteId { get; set; }
        public string RouteName { get; set; } = string.Empty;
        public List<Delivery> OptimizedDeliveries { get; set; } = new();
        public decimal RouteDistance { get; set; }
        public TimeSpan EstimatedDuration { get; set; }
        public int DeliveryCount { get; set; }
        public decimal UtilizationPercentage { get; set; }
    }

    /// <summary>
    /// Traffic conditions information.
    /// </summary>
    public class TrafficConditions
    {
        public TrafficLevel OverallTrafficLevel { get; set; }
        public List<TrafficSegment> TrafficSegments { get; set; } = new();
        public TimeSpan EstimatedDelay { get; set; }
        public string[] Incidents { get; set; } = Array.Empty<string>();
        public DateTime LastUpdated { get; set; }
    }

    /// <summary>
    /// Traffic level enumeration.
    /// </summary>
    public enum TrafficLevel
    {
        Light,
        Moderate,
        Heavy,
        Severe
    }

    /// <summary>
    /// Traffic segment information.
    /// </summary>
    public class TrafficSegment
    {
        public double FromLatitude { get; set; }
        public double FromLongitude { get; set; }
        public double ToLatitude { get; set; }
        public double ToLongitude { get; set; }
        public TrafficLevel TrafficLevel { get; set; }
        public TimeSpan EstimatedDelay { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    /// <summary>
    /// Delivery location information.
    /// </summary>
    public class DeliveryLocation
    {
        public int DeliveryId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Address { get; set; } = string.Empty;
        public DateTime TimeWindow { get; set; }
    }

    /// <summary>
    /// Alternative route suggestion.
    /// </summary>
    public class AlternativeRoute
    {
        public int RouteId { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal AdditionalDistance { get; set; }
        public TimeSpan AdditionalTime { get; set; }
        public decimal CostImpact { get; set; }
        public List<Delivery> AffectedDeliveries { get; set; } = new();
        public string Reason { get; set; } = string.Empty;
    }

    /// <summary>
    /// Route issue types.
    /// </summary>
    public class RouteIssue
    {
        public RouteIssueType Type { get; set; }
        public string Description { get; set; } = string.Empty;
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public DateTime ReportedAt { get; set; }
        public TimeSpan EstimatedDuration { get; set; }
    }

    /// <summary>
    /// Route issue type enumeration.
    /// </summary>
    public enum RouteIssueType
    {
        TrafficJam,
        RoadClosure,
        VehicleBreakdown,
        DeliveryFailure,
        WeatherConditions,
        Other
    }

    /// <summary>
    /// Route validation result.
    /// </summary>
    public class RouteValidationResult
    {
        public bool IsValid { get; set; }
        public List<ValidationIssue> Issues { get; set; } = new();
        public RouteCapacityAnalysis CapacityAnalysis { get; set; } = new();
        public TimeWindowAnalysis TimeWindowAnalysis { get; set; } = new();
        public string Summary { get; set; } = string.Empty;
    }

    /// <summary>
    /// Validation issue information.
    /// </summary>
    public class ValidationIssue
    {
        public ValidationIssueType Type { get; set; }
        public string Message { get; set; } = string.Empty;
        public int? DeliveryId { get; set; }
        public ValidationSeverity Severity { get; set; }
    }

    /// <summary>
    /// Validation issue types.
    /// </summary>
    public enum ValidationIssueType
    {
        CapacityExceeded,
        TimeWindowViolation,
        UnreachableLocation,
        DuplicateDelivery,
        InvalidSequence
    }

    /// <summary>
    /// Validation severity levels.
    /// </summary>
    public enum ValidationSeverity
    {
        Info,
        Warning,
        Error,
        Critical
    }

    /// <summary>
    /// Route capacity analysis.
    /// </summary>
    public class RouteCapacityAnalysis
    {
        public decimal TotalWeight { get; set; }
        public decimal TotalVolume { get; set; }
        public int TotalItems { get; set; }
        public decimal WeightUtilization { get; set; }
        public decimal VolumeUtilization { get; set; }
        public bool IsOverCapacity { get; set; }
    }

    /// <summary>
    /// Time window analysis.
    /// </summary>
    public class TimeWindowAnalysis
    {
        public int TotalDeliveries { get; set; }
        public int DeliveriesWithTimeWindows { get; set; }
        public int TimeWindowViolations { get; set; }
        public TimeSpan EarliestStart { get; set; }
        public TimeSpan LatestFinish { get; set; }
        public bool HasConflicts { get; set; }
    }

    /// <summary>
    /// Route performance metrics.
    /// </summary>
    public class RoutePerformanceMetrics
    {
        public int RouteId { get; set; }
        public string RouteName { get; set; } = string.Empty;
        public int TotalDeliveries { get; set; }
        public int SuccessfulDeliveries { get; set; }
        public int FailedDeliveries { get; set; }
        public decimal SuccessRate { get; set; }
        public TimeSpan AverageDeliveryTime { get; set; }
        public decimal AverageDistance { get; set; }
        public decimal FuelConsumption { get; set; }
        public decimal OperatingCost { get; set; }
        public decimal CustomerSatisfactionScore { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
    }
}