using VHouse.Domain.Entities;

namespace VHouse.Application.Services;

public interface IAuditService
{
    Task LogActionAsync(string action, string entityType, int? entityId, string userId, string userName, 
                       object? oldValues = null, object? newValues = null, string? changes = null,
                       string severity = "INFO", string moduleName = "SYSTEM", decimal? amountInvolved = null,
                       string? clientTenant = null, bool isSuccess = true, string? errorMessage = null,
                       TimeSpan? executionTime = null);
    
    Task LogBusinessEventAsync(string action, string entityType, int? entityId, string userId, 
                              string userName, decimal? amountInvolved = null, string? clientTenant = null);
    
    Task LogSecurityEventAsync(string action, string userId, string userName, string? details = null,
                              string severity = "WARNING", bool isSuccess = true);
    
    Task LogPerformanceAsync(string operation, TimeSpan executionTime, string? moduleName = null,
                            string? additionalData = null);
    
    Task<List<AuditLog>> GetAuditHistoryAsync(string? entityType = null, int? entityId = null,
                                             DateTime? fromDate = null, DateTime? toDate = null,
                                             string? userId = null, int pageSize = 50, int page = 1);
    
    Task<List<AuditLog>> GetSecurityEventsAsync(DateTime? fromDate = null, DateTime? toDate = null,
                                               string severity = "WARNING", int pageSize = 50);
    
    Task<List<AuditLog>> GetBusinessEventsAsync(string? clientTenant = null, DateTime? fromDate = null, 
                                               DateTime? toDate = null, int pageSize = 50);
}