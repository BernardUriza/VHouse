using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using VHouse.Classes;
using VHouse.Interfaces;
using VHouse.Extensions;

namespace VHouse.Services
{
    /// <summary>
    /// Service for tenant management operations.
    /// </summary>
    public class TenantService : ITenantService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TenantService> _logger;

        public TenantService(ApplicationDbContext context, ILogger<TenantService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Gets all active tenants with pagination.
        /// </summary>
        public async Task<PagedResult<Tenant>> GetActiveTenantsAsync(int page = 1, int pageSize = 20)
        {
            try
            {
                var query = _context.Tenants
                    .AsNoTracking()
                    .Where(t => t.IsActive)
                    .OrderBy(t => t.Name);

                var pagedResult = await query.ToPagedResultAsync(page, pageSize);
                
                _logger.LogInformation("Retrieved {Count} active tenants for page {Page}", 
                    pagedResult.Items.Count, page);
                
                return pagedResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active tenants for page {Page}", page);
                throw;
            }
        }

        /// <summary>
        /// Gets a tenant by ID including its distribution centers.
        /// </summary>
        public async Task<Tenant?> GetTenantByIdAsync(int tenantId, bool includeDistributionCenters = false)
        {
            try
            {
                var query = _context.Tenants.AsNoTracking();

                if (includeDistributionCenters)
                {
                    query = query.Include(t => t.DistributionCenters.Where(dc => dc.IsActive))
                               .ThenInclude(dc => dc.Warehouses.Where(w => w.IsActive));
                }

                var tenant = await query.FirstOrDefaultAsync(t => t.TenantId == tenantId);

                if (tenant != null)
                {
                    _logger.LogInformation("Retrieved tenant {TenantId} - {TenantName}", 
                        tenant.TenantId, tenant.Name);
                }
                else
                {
                    _logger.LogWarning("Tenant with ID {TenantId} not found", tenantId);
                }

                return tenant;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tenant with ID {TenantId}", tenantId);
                throw;
            }
        }

        /// <summary>
        /// Gets a tenant by tenant code.
        /// </summary>
        public async Task<Tenant?> GetTenantByCodeAsync(string tenantCode)
        {
            try
            {
                var tenant = await _context.Tenants
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.TenantCode == tenantCode && t.IsActive);

                if (tenant != null)
                {
                    _logger.LogInformation("Retrieved tenant by code {TenantCode} - {TenantName}", 
                        tenantCode, tenant.Name);
                }
                else
                {
                    _logger.LogWarning("Tenant with code {TenantCode} not found", tenantCode);
                }

                return tenant;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tenant with code {TenantCode}", tenantCode);
                throw;
            }
        }

        /// <summary>
        /// Creates a new tenant.
        /// </summary>
        public async Task<Tenant> CreateTenantAsync(Tenant tenant)
        {
            try
            {
                // Check for unique tenant code
                var existingTenant = await _context.Tenants
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.TenantCode == tenant.TenantCode);

                if (existingTenant != null)
                {
                    throw new InvalidOperationException($"Tenant with code {tenant.TenantCode} already exists");
                }

                tenant.CreatedDate = DateTime.UtcNow;
                tenant.LastUpdated = DateTime.UtcNow;

                _context.Tenants.Add(tenant);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created new tenant {TenantCode} - {TenantName} with ID {TenantId}", 
                    tenant.TenantCode, tenant.Name, tenant.TenantId);

                return tenant;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating tenant {TenantCode}", tenant.TenantCode);
                throw;
            }
        }

        /// <summary>
        /// Updates an existing tenant.
        /// </summary>
        public async Task<bool> UpdateTenantAsync(Tenant tenant)
        {
            try
            {
                var existingTenant = await _context.Tenants
                    .FirstOrDefaultAsync(t => t.TenantId == tenant.TenantId);

                if (existingTenant == null)
                {
                    _logger.LogWarning("Tenant with ID {TenantId} not found for update", tenant.TenantId);
                    return false;
                }

                // Check for unique tenant code (excluding current tenant)
                var duplicateCode = await _context.Tenants
                    .AsNoTracking()
                    .AnyAsync(t => t.TenantCode == tenant.TenantCode && t.TenantId != tenant.TenantId);

                if (duplicateCode)
                {
                    throw new InvalidOperationException($"Tenant with code {tenant.TenantCode} already exists");
                }

                existingTenant.TenantCode = tenant.TenantCode;
                existingTenant.Name = tenant.Name;
                existingTenant.Description = tenant.Description;
                existingTenant.ContactEmail = tenant.ContactEmail;
                existingTenant.ContactPhone = tenant.ContactPhone;
                existingTenant.Address = tenant.Address;
                existingTenant.Configuration = tenant.Configuration;
                existingTenant.IsActive = tenant.IsActive;
                existingTenant.LastUpdated = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated tenant {TenantCode} - {TenantName}", 
                    tenant.TenantCode, tenant.Name);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating tenant {TenantId}", tenant.TenantId);
                throw;
            }
        }

        /// <summary>
        /// Deactivates a tenant and all its distribution centers.
        /// </summary>
        public async Task<bool> DeactivateTenantAsync(int tenantId)
        {
            try
            {
                var tenant = await _context.Tenants
                    .Include(t => t.DistributionCenters)
                    .ThenInclude(dc => dc.DeliveryRoutes)
                    .FirstOrDefaultAsync(t => t.TenantId == tenantId);

                if (tenant == null)
                {
                    _logger.LogWarning("Tenant with ID {TenantId} not found for deactivation", tenantId);
                    return false;
                }

                tenant.IsActive = false;
                tenant.LastUpdated = DateTime.UtcNow;

                // Deactivate all distribution centers and routes
                foreach (var center in tenant.DistributionCenters)
                {
                    center.IsActive = false;
                    foreach (var route in center.DeliveryRoutes)
                    {
                        route.IsActive = false;
                    }
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Deactivated tenant {TenantCode} and all its distribution centers", 
                    tenant.TenantCode);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating tenant {TenantId}", tenantId);
                throw;
            }
        }

        /// <summary>
        /// Gets tenant configuration as typed object.
        /// </summary>
        public async Task<T?> GetTenantConfigurationAsync<T>(int tenantId) where T : class
        {
            try
            {
                var tenant = await _context.Tenants
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.TenantId == tenantId);

                if (tenant == null || string.IsNullOrEmpty(tenant.Configuration))
                {
                    return null;
                }

                var configuration = JsonSerializer.Deserialize<T>(tenant.Configuration);
                
                _logger.LogInformation("Retrieved configuration for tenant {TenantId}", tenantId);
                
                return configuration;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving configuration for tenant {TenantId}", tenantId);
                throw;
            }
        }

        /// <summary>
        /// Updates tenant configuration.
        /// </summary>
        public async Task<bool> UpdateTenantConfigurationAsync<T>(int tenantId, T configuration) where T : class
        {
            try
            {
                var tenant = await _context.Tenants
                    .FirstOrDefaultAsync(t => t.TenantId == tenantId);

                if (tenant == null)
                {
                    _logger.LogWarning("Tenant with ID {TenantId} not found for configuration update", tenantId);
                    return false;
                }

                tenant.Configuration = JsonSerializer.Serialize(configuration);
                tenant.LastUpdated = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated configuration for tenant {TenantId}", tenantId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating configuration for tenant {TenantId}", tenantId);
                throw;
            }
        }

        /// <summary>
        /// Gets tenant statistics and metrics.
        /// </summary>
        public async Task<TenantStatistics> GetTenantStatisticsAsync(int tenantId)
        {
            try
            {
                var tenant = await _context.Tenants
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.TenantId == tenantId);

                if (tenant == null)
                {
                    throw new ArgumentException($"Tenant with ID {tenantId} not found");
                }

                var distributionCenters = await _context.DistributionCenters
                    .AsNoTracking()
                    .CountAsync(dc => dc.TenantId == tenantId && dc.IsActive);

                var activeRoutes = await _context.DeliveryRoutes
                    .AsNoTracking()
                    .CountAsync(dr => dr.TenantId == tenantId && dr.IsActive);

                var deliveryStats = await _context.Deliveries
                    .AsNoTracking()
                    .Where(d => d.DeliveryRoute!.TenantId == tenantId)
                    .GroupBy(d => d.Status)
                    .Select(g => new { Status = g.Key, Count = g.Count() })
                    .ToListAsync();

                var totalDeliveries = deliveryStats.Sum(s => s.Count);
                var pendingDeliveries = deliveryStats.Where(s => s.Status == "Pending" || s.Status == "InTransit").Sum(s => s.Count);
                var completedDeliveries = deliveryStats.Where(s => s.Status == "Delivered").Sum(s => s.Count);

                var averageDeliveryTime = await _context.Deliveries
                    .AsNoTracking()
                    .Where(d => d.DeliveryRoute!.TenantId == tenantId && 
                               d.Status == "Delivered" && 
                               d.ActualDeliveryDate.HasValue)
                    .Select(d => EF.Functions.DateDiffMinute(d.ScheduledDate, d.ActualDeliveryDate!.Value))
                    .DefaultIfEmpty(0)
                    .AverageAsync();

                var lastActivity = await _context.Deliveries
                    .AsNoTracking()
                    .Where(d => d.DeliveryRoute!.TenantId == tenantId)
                    .OrderByDescending(d => d.ActualDeliveryDate ?? d.ScheduledDate)
                    .Select(d => d.ActualDeliveryDate ?? d.ScheduledDate)
                    .FirstOrDefaultAsync();

                var statistics = new TenantStatistics
                {
                    TenantId = tenantId,
                    TenantName = tenant.Name,
                    DistributionCenters = distributionCenters,
                    ActiveRoutes = activeRoutes,
                    TotalDeliveries = totalDeliveries,
                    PendingDeliveries = pendingDeliveries,
                    CompletedDeliveries = completedDeliveries,
                    AverageDeliveryTime = (decimal)averageDeliveryTime,
                    LastActivity = lastActivity
                };

                _logger.LogInformation("Retrieved statistics for tenant {TenantId} - {TenantName}", 
                    tenantId, tenant.Name);

                return statistics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving statistics for tenant {TenantId}", tenantId);
                throw;
            }
        }
    }
}