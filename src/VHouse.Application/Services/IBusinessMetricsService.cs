using VHouse.Domain.Entities;

namespace VHouse.Application.Services;

public interface IBusinessMetricsService
{
    // Real-time performance metrics
    Task RecordMetricAsync(string metricName, string metricType, decimal value, string? unit = null,
                          string? source = null, string? clientTenant = null, string severity = "NORMAL",
                          bool requiresAction = false, string? actionRequired = null, string? metadata = null);
    
    Task RecordPerformanceAsync(string operation, TimeSpan executionTime, string? source = null);
    Task RecordBusinessEventAsync(string eventType, decimal value, string? clientTenant = null, string? metadata = null);
    Task RecordInventoryMetricAsync(string productName, int quantity, string operation, string? clientTenant = null);
    Task RecordSalesMetricAsync(decimal amount, string clientTenant, int itemCount);
    
    // Alert system
    Task CreateAlertAsync(string alertType, string title, string description, string severity,
                         string? clientTenant = null, string? relatedEntity = null, int? relatedEntityId = null,
                         decimal? amountInvolved = null, string? actionData = null);
    
    Task<List<BusinessAlert>> GetActiveAlertsAsync(string? clientTenant = null, string? severity = null);
    Task<List<BusinessAlert>> GetCriticalAlertsAsync();
    Task ResolveAlertAsync(int alertId, string resolvedBy, string? resolutionNotes = null);
    
    // Metrics retrieval
    Task<List<SystemMetric>> GetMetricsAsync(string? metricType = null, string? source = null,
                                           DateTime? fromDate = null, DateTime? toDate = null,
                                           string? clientTenant = null, int pageSize = 100);
    
    Task<decimal> GetAverageMetricAsync(string metricName, DateTime? fromDate = null, DateTime? toDate = null,
                                      string? clientTenant = null);
    
    Task<List<SystemMetric>> GetPerformanceMetricsAsync(DateTime? fromDate = null, DateTime? toDate = null);
    Task<List<SystemMetric>> GetBusinessMetricsAsync(string? clientTenant = null, DateTime? fromDate = null, DateTime? toDate = null);
    
    // Real-time monitoring
    Task CheckInventoryLevelsAsync();
    Task CheckPerformanceThresholdsAsync();
    Task CheckBusinessAnomaliesAsync();
    
    // Dashboard data
    Task<object> GetDashboardDataAsync(string? clientTenant = null, DateTime? fromDate = null, DateTime? toDate = null);
    Task<object> GetRealTimeStatusAsync();
}