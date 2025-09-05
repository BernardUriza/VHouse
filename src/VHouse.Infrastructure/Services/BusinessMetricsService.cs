using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using VHouse.Application.Services;
using VHouse.Domain.Entities;
using VHouse.Infrastructure.Data;

namespace VHouse.Infrastructure.Services;

public class BusinessMetricsService : IBusinessMetricsService
{
    private readonly VHouseDbContext _context;
    private readonly IAuditService _auditService;
    private readonly ILogger<BusinessMetricsService> _logger;

    public BusinessMetricsService(VHouseDbContext context, IAuditService auditService, ILogger<BusinessMetricsService> logger)
    {
        _context = context;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task RecordMetricAsync(string metricName, string metricType, decimal value, string? unit = null,
                                      string? source = null, string? clientTenant = null, string severity = "NORMAL",
                                      bool requiresAction = false, string? actionRequired = null, string? metadata = null)
    {
        try
        {
            var metric = new SystemMetric
            {
                MetricName = metricName.Length > 100 ? metricName.Substring(0, 100) : metricName,
                MetricType = metricType.Length > 50 ? metricType.Substring(0, 50) : metricType,
                Value = value,
                Unit = unit?.Length > 20 ? unit.Substring(0, 20) : unit ?? string.Empty,
                Source = source?.Length > 100 ? source.Substring(0, 100) : source,
                ClientTenant = clientTenant?.Length > 100 ? clientTenant.Substring(0, 100) : clientTenant,
                Severity = severity,
                RequiresAction = requiresAction,
                ActionRequired = actionRequired?.Length > 500 ? actionRequired.Substring(0, 500) : actionRequired,
                Metadata = metadata,
                Timestamp = DateTime.UtcNow
            };

            _context.SystemMetrics.Add(metric);
            await _context.SaveChangesAsync();

            // Auto-generate alerts for critical metrics
            if (severity == "CRITICAL" && requiresAction)
            {
                await CreateAlertAsync(metricType, $"Critical Metric: {metricName}", 
                    $"Metric {metricName} has reached critical value: {value} {unit}",
                    "CRITICAL", clientTenant, "SystemMetric", metric.Id, value);
            }

            await _auditService.LogActionAsync("METRIC_RECORDED", "SystemMetric", metric.Id, "SYSTEM", "Metrics Service",
                null, new { metricName, metricType, value, unit }, 
                $"Recorded {metricType} metric: {metricName} = {value} {unit}", "INFO", "METRICS");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to record metric {MetricName}: {MetricType} = {Value}", metricName, metricType, value);
        }
    }

    public async Task RecordPerformanceAsync(string operation, TimeSpan executionTime, string? source = null)
    {
        await RecordMetricAsync($"PERF_{operation}", "PERFORMANCE", (decimal)executionTime.TotalMilliseconds, "ms", 
            source ?? "APPLICATION", null, executionTime.TotalMilliseconds > 5000 ? "WARNING" : "NORMAL",
            executionTime.TotalMilliseconds > 10000, 
            executionTime.TotalMilliseconds > 10000 ? "Investigate slow operation" : null);
    }

    public async Task RecordBusinessEventAsync(string eventType, decimal value, string? clientTenant = null, string? metadata = null)
    {
        await RecordMetricAsync($"BIZ_{eventType}", "BUSINESS", value, "units", "BUSINESS_LOGIC", clientTenant, "INFO", false, null, metadata);
    }

    public async Task RecordInventoryMetricAsync(string productName, int quantity, string operation, string? clientTenant = null)
    {
        var severity = quantity <= 5 && operation == "STOCK_LEVEL" ? "WARNING" : "NORMAL";
        var requiresAction = quantity <= 2 && operation == "STOCK_LEVEL";
        
        await RecordMetricAsync($"INV_{operation}_{productName}", "INVENTORY", quantity, "units", "INVENTORY_SYSTEM",
            clientTenant, severity, requiresAction, 
            requiresAction ? $"Low stock alert for {productName} - only {quantity} units remaining" : null,
            JsonSerializer.Serialize(new { productName, operation, clientTenant }));
    }

    public async Task RecordSalesMetricAsync(decimal amount, string clientTenant, int itemCount)
    {
        await RecordMetricAsync($"SALES_AMOUNT", "BUSINESS", amount, "MXN", "POS_SYSTEM", clientTenant, "INFO");
        await RecordMetricAsync($"SALES_ITEMS", "BUSINESS", itemCount, "items", "POS_SYSTEM", clientTenant, "INFO");
    }

    public async Task CreateAlertAsync(string alertType, string title, string description, string severity,
                                     string? clientTenant = null, string? relatedEntity = null, int? relatedEntityId = null,
                                     decimal? amountInvolved = null, string? actionData = null)
    {
        try
        {
            var alert = new BusinessAlert
            {
                AlertType = alertType.Length > 20 ? alertType.Substring(0, 20) : alertType,
                Title = title.Length > 200 ? title.Substring(0, 200) : title,
                Description = description.Length > 1000 ? description.Substring(0, 1000) : description,
                Severity = severity,
                ClientTenant = clientTenant?.Length > 100 ? clientTenant.Substring(0, 100) : clientTenant,
                RelatedEntity = relatedEntity?.Length > 100 ? relatedEntity.Substring(0, 100) : relatedEntity,
                RelatedEntityId = relatedEntityId,
                AmountInvolved = amountInvolved,
                ActionData = actionData,
                CreatedAt = DateTime.UtcNow,
                IsResolved = false
            };

            _context.BusinessAlerts.Add(alert);
            await _context.SaveChangesAsync();

            await _auditService.LogActionAsync("ALERT_CREATED", "BusinessAlert", alert.Id, "SYSTEM", "Metrics Service",
                null, new { alertType, severity, title }, 
                $"Created {severity} alert: {title}", severity == "CRITICAL" ? "ERROR" : "WARNING", "ALERTS");

            if (severity == "CRITICAL")
            {
                _logger.LogError("CRITICAL ALERT Created: {Title} - {Description}", title, description);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create alert {AlertType}: {Title}", alertType, title);
        }
    }

    public async Task<List<BusinessAlert>> GetActiveAlertsAsync(string? clientTenant = null, string? severity = null)
    {
        var query = _context.BusinessAlerts.Where(a => !a.IsResolved);

        if (!string.IsNullOrEmpty(clientTenant))
            query = query.Where(a => a.ClientTenant == clientTenant);

        if (!string.IsNullOrEmpty(severity))
            query = query.Where(a => a.Severity == severity);

        return await query.OrderByDescending(a => a.CreatedAt).ToListAsync();
    }

    public async Task<List<BusinessAlert>> GetCriticalAlertsAsync()
    {
        return await _context.BusinessAlerts
            .Where(a => !a.IsResolved && a.Severity == "CRITICAL")
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task ResolveAlertAsync(int alertId, string resolvedBy, string? resolutionNotes = null)
    {
        var alert = await _context.BusinessAlerts.FindAsync(alertId);
        if (alert != null && !alert.IsResolved)
        {
            alert.IsResolved = true;
            alert.ResolvedAt = DateTime.UtcNow;
            alert.ResolvedBy = resolvedBy.Length > 200 ? resolvedBy.Substring(0, 200) : resolvedBy;
            alert.ResolutionNotes = resolutionNotes?.Length > 500 ? resolutionNotes.Substring(0, 500) : resolutionNotes;

            await _context.SaveChangesAsync();

            await _auditService.LogActionAsync("ALERT_RESOLVED", "BusinessAlert", alertId, resolvedBy, resolvedBy,
                new { isResolved = false }, new { isResolved = true, resolutionNotes },
                $"Resolved alert: {alert.Title}", "INFO", "ALERTS");
        }
    }

    public async Task<List<SystemMetric>> GetMetricsAsync(string? metricType = null, string? source = null,
                                                        DateTime? fromDate = null, DateTime? toDate = null,
                                                        string? clientTenant = null, int pageSize = 100)
    {
        var query = _context.SystemMetrics.AsQueryable();

        if (!string.IsNullOrEmpty(metricType))
            query = query.Where(m => m.MetricType == metricType);

        if (!string.IsNullOrEmpty(source))
            query = query.Where(m => m.Source == source);

        if (!string.IsNullOrEmpty(clientTenant))
            query = query.Where(m => m.ClientTenant == clientTenant);

        if (fromDate.HasValue)
            query = query.Where(m => m.Timestamp >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(m => m.Timestamp <= toDate.Value);

        return await query
            .OrderByDescending(m => m.Timestamp)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<decimal> GetAverageMetricAsync(string metricName, DateTime? fromDate = null, DateTime? toDate = null,
                                                   string? clientTenant = null)
    {
        var query = _context.SystemMetrics.Where(m => m.MetricName == metricName);

        if (!string.IsNullOrEmpty(clientTenant))
            query = query.Where(m => m.ClientTenant == clientTenant);

        if (fromDate.HasValue)
            query = query.Where(m => m.Timestamp >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(m => m.Timestamp <= toDate.Value);

        return await query.AverageAsync(m => (decimal?)m.Value) ?? 0;
    }

    public async Task<List<SystemMetric>> GetPerformanceMetricsAsync(DateTime? fromDate = null, DateTime? toDate = null)
    {
        return await GetMetricsAsync("PERFORMANCE", null, fromDate, toDate, null, 100);
    }

    public async Task<List<SystemMetric>> GetBusinessMetricsAsync(string? clientTenant = null, DateTime? fromDate = null, DateTime? toDate = null)
    {
        return await GetMetricsAsync("BUSINESS", null, fromDate, toDate, clientTenant, 100);
    }

    public async Task CheckInventoryLevelsAsync()
    {
        try
        {
            var lowStockProducts = await _context.Products
                .Where(p => p.IsActive && p.StockQuantity <= 5)
                .ToListAsync();

            foreach (var product in lowStockProducts)
            {
                await RecordInventoryMetricAsync(product.ProductName, product.StockQuantity, "STOCK_LEVEL");
                
                if (product.StockQuantity <= 2)
                {
                    await CreateAlertAsync("STOCK_LOW", $"Low Stock: {product.ProductName}",
                        $"Product {product.ProductName} has only {product.StockQuantity} units remaining",
                        "HIGH", null, "Product", product.Id);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check inventory levels");
        }
    }

    public async Task CheckPerformanceThresholdsAsync()
    {
        try
        {
            var recentPerformanceMetrics = await _context.SystemMetrics
                .Where(m => m.MetricType == "PERFORMANCE" && m.Timestamp >= DateTime.UtcNow.AddMinutes(-10))
                .ToListAsync();

            var slowOperations = recentPerformanceMetrics.Where(m => m.Value > 5000).ToList();
            
            foreach (var slowOp in slowOperations)
            {
                if (slowOp.Value > 10000) // > 10 seconds
                {
                    await CreateAlertAsync("PERFORMANCE", $"Slow Operation: {slowOp.MetricName}",
                        $"Operation {slowOp.MetricName} took {slowOp.Value}ms to complete",
                        "HIGH", null, "SystemMetric", slowOp.Id, slowOp.Value);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check performance thresholds");
        }
    }

    public async Task CheckBusinessAnomaliesAsync()
    {
        try
        {
            var today = DateTime.UtcNow.Date;
            var todaySales = await _context.SystemMetrics
                .Where(m => m.MetricName == "SALES_AMOUNT" && m.Timestamp >= today)
                .SumAsync(m => m.Value);

            var avgDailySales = await _context.SystemMetrics
                .Where(m => m.MetricName == "SALES_AMOUNT" && m.Timestamp >= DateTime.UtcNow.AddDays(-30))
                .GroupBy(m => m.Timestamp.Date)
                .Select(g => g.Sum(m => m.Value))
                .DefaultIfEmpty(0)
                .AverageAsync();

            if (todaySales > avgDailySales * 2)
            {
                await CreateAlertAsync("SALES_HIGH", "Unusually High Sales",
                    $"Today's sales ({todaySales:C}) are significantly higher than average ({avgDailySales:C})",
                    "MEDIUM", null, null, null, todaySales);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check business anomalies");
        }
    }

    public async Task<object> GetDashboardDataAsync(string? clientTenant = null, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var from = fromDate ?? DateTime.UtcNow.AddDays(-7);
        var to = toDate ?? DateTime.UtcNow;

        var metrics = await GetMetricsAsync(null, null, from, to, clientTenant, 1000);
        var alerts = await GetActiveAlertsAsync(clientTenant);
        
        var salesMetrics = metrics.Where(m => m.MetricName == "SALES_AMOUNT").ToList();
        var inventoryMetrics = metrics.Where(m => m.MetricType == "INVENTORY").ToList();
        var performanceMetrics = metrics.Where(m => m.MetricType == "PERFORMANCE").ToList();

        return new
        {
            TotalSales = salesMetrics.Sum(m => m.Value),
            AverageSalePerDay = salesMetrics.GroupBy(m => m.Timestamp.Date).Average(g => g.Sum(m => m.Value)),
            ActiveAlerts = alerts.Count,
            CriticalAlerts = alerts.Count(a => a.Severity == "CRITICAL"),
            AveragePerformance = performanceMetrics.Any() ? performanceMetrics.Average(m => m.Value) : 0,
            LowStockProducts = inventoryMetrics.Where(m => m.Value <= 5).Count(),
            MetricsCollected = metrics.Count,
            ClientTenant = clientTenant,
            DateRange = new { From = from, To = to }
        };
    }

    public async Task<object> GetRealTimeStatusAsync()
    {
        var criticalAlerts = await GetCriticalAlertsAsync();
        var recentMetrics = await GetMetricsAsync(null, null, DateTime.UtcNow.AddMinutes(-5), null, null, 50);
        
        return new
        {
            Status = criticalAlerts.Any() ? "CRITICAL" : "HEALTHY",
            CriticalAlerts = criticalAlerts.Count,
            RecentMetrics = recentMetrics.Count,
            LastUpdate = DateTime.UtcNow,
            SystemHealth = new
            {
                Database = "CONNECTED",
                Performance = recentMetrics.Where(m => m.MetricType == "PERFORMANCE").Any(m => m.Value > 10000) ? "DEGRADED" : "GOOD",
                Inventory = recentMetrics.Where(m => m.MetricType == "INVENTORY").Any(m => m.Value <= 2) ? "LOW_STOCK" : "NORMAL"
            }
        };
    }
}