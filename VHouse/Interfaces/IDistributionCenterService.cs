using VHouse.Classes;

namespace VHouse.Interfaces
{
    /// <summary>
    /// Service interface for distribution center management operations.
    /// </summary>
    public interface IDistributionCenterService
    {
        /// <summary>
        /// Gets all distribution centers for a tenant with pagination.
        /// </summary>
        Task<PagedResult<DistributionCenter>> GetDistributionCentersByTenantAsync(int tenantId, int page = 1, int pageSize = 20);

        /// <summary>
        /// Gets active distribution centers within a geographic radius.
        /// </summary>
        Task<List<DistributionCenter>> GetDistributionCentersInRadiusAsync(double latitude, double longitude, double radiusKm);

        /// <summary>
        /// Gets a distribution center by ID including related data.
        /// </summary>
        Task<DistributionCenter?> GetDistributionCenterByIdAsync(int distributionCenterId, bool includeWarehouses = false, bool includeRoutes = false);

        /// <summary>
        /// Creates a new distribution center.
        /// </summary>
        Task<DistributionCenter> CreateDistributionCenterAsync(DistributionCenter distributionCenter);

        /// <summary>
        /// Updates an existing distribution center.
        /// </summary>
        Task<bool> UpdateDistributionCenterAsync(DistributionCenter distributionCenter);

        /// <summary>
        /// Deactivates a distribution center and all its routes.
        /// </summary>
        Task<bool> DeactivateDistributionCenterAsync(int distributionCenterId);

        /// <summary>
        /// Gets distribution center capacity utilization.
        /// </summary>
        Task<DistributionCenterCapacity> GetDistributionCenterCapacityAsync(int distributionCenterId);

        /// <summary>
        /// Gets optimal distribution center for a delivery location.
        /// </summary>
        Task<DistributionCenter?> GetOptimalDistributionCenterAsync(double deliveryLatitude, double deliveryLongitude, int tenantId);

        /// <summary>
        /// Gets distribution center performance metrics.
        /// </summary>
        Task<DistributionCenterMetrics> GetDistributionCenterMetricsAsync(int distributionCenterId, DateTime? fromDate = null, DateTime? toDate = null);
    }

    /// <summary>
    /// Distribution center capacity information.
    /// </summary>
    public class DistributionCenterCapacity
    {
        public int DistributionCenterId { get; set; }
        public string CenterName { get; set; } = string.Empty;
        public int MaxCapacity { get; set; }
        public int CurrentLoad { get; set; }
        public decimal UtilizationPercentage { get; set; }
        public int AvailableCapacity { get; set; }
        public bool IsOverCapacity { get; set; }
    }

    /// <summary>
    /// Distribution center performance metrics.
    /// </summary>
    public class DistributionCenterMetrics
    {
        public int DistributionCenterId { get; set; }
        public string CenterName { get; set; } = string.Empty;
        public int TotalDeliveries { get; set; }
        public int CompletedDeliveries { get; set; }
        public int FailedDeliveries { get; set; }
        public decimal DeliverySuccessRate { get; set; }
        public decimal AverageDeliveryTime { get; set; }
        public int ActiveRoutes { get; set; }
        public int TotalWarehouses { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
    }
}