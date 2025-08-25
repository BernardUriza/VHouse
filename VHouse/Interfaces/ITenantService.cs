using VHouse.Classes;

namespace VHouse.Interfaces
{
    /// <summary>
    /// Service interface for tenant management operations.
    /// </summary>
    public interface ITenantService
    {
        /// <summary>
        /// Gets all active tenants with pagination.
        /// </summary>
        Task<PagedResult<Tenant>> GetActiveTenantsAsync(int page = 1, int pageSize = 20);

        /// <summary>
        /// Gets a tenant by ID including its distribution centers.
        /// </summary>
        Task<Tenant?> GetTenantByIdAsync(int tenantId, bool includeDistributionCenters = false);

        /// <summary>
        /// Gets a tenant by tenant code.
        /// </summary>
        Task<Tenant?> GetTenantByCodeAsync(string tenantCode);

        /// <summary>
        /// Creates a new tenant.
        /// </summary>
        Task<Tenant> CreateTenantAsync(Tenant tenant);

        /// <summary>
        /// Updates an existing tenant.
        /// </summary>
        Task<bool> UpdateTenantAsync(Tenant tenant);

        /// <summary>
        /// Deactivates a tenant and all its distribution centers.
        /// </summary>
        Task<bool> DeactivateTenantAsync(int tenantId);

        /// <summary>
        /// Gets tenant configuration as typed object.
        /// </summary>
        Task<T?> GetTenantConfigurationAsync<T>(int tenantId) where T : class;

        /// <summary>
        /// Updates tenant configuration.
        /// </summary>
        Task<bool> UpdateTenantConfigurationAsync<T>(int tenantId, T configuration) where T : class;

        /// <summary>
        /// Gets tenant statistics and metrics.
        /// </summary>
        Task<TenantStatistics> GetTenantStatisticsAsync(int tenantId);
    }

    /// <summary>
    /// Tenant statistics data structure.
    /// </summary>
    public class TenantStatistics
    {
        public int TenantId { get; set; }
        public string TenantName { get; set; } = string.Empty;
        public int DistributionCenters { get; set; }
        public int ActiveRoutes { get; set; }
        public int TotalDeliveries { get; set; }
        public int PendingDeliveries { get; set; }
        public int CompletedDeliveries { get; set; }
        public decimal AverageDeliveryTime { get; set; }
        public DateTime LastActivity { get; set; }
    }
}