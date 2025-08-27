using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text.Json;
using VHouse.Interfaces;

namespace VHouse.Services
{
    /// <summary>
    /// Production monitoring service for observability, logging, and performance tracking.
    /// </summary>
    public class MonitoringService : IMonitoringService
    {
        private readonly ILogger<MonitoringService> _logger;
        private readonly ICachingService _cachingService;
        private readonly IConfiguration _configuration;
        private readonly Dictionary<string, List<double>> _metricsBuffer = new();
        private readonly List<MonitoringAlert> _activeAlerts = new();
        private readonly DateTime _startTime = DateTime.UtcNow;

        public MonitoringService(
            ILogger<MonitoringService> logger,
            ICachingService cachingService,
            IConfiguration configuration)
        {
            _logger = logger;
            _cachingService = cachingService;
            _configuration = configuration;
        }

        /// <summary>
        /// Records application metrics for monitoring and alerting.
        /// </summary>
        public async Task RecordMetricAsync(string metricName, double value, Dictionary<string, string>? tags = null)
        {
            try
            {
                var metric = new
                {
                    Name = metricName,
                    Value = value,
                    Tags = tags ?? new Dictionary<string, string>(),
                    Timestamp = DateTime.UtcNow
                };

                // Buffer metrics for batch processing
                lock (_metricsBuffer)
                {
                    if (!_metricsBuffer.ContainsKey(metricName))
                    {
                        _metricsBuffer[metricName] = new List<double>();
                    }
                    _metricsBuffer[metricName].Add(value);

                    // Keep buffer size manageable
                    if (_metricsBuffer[metricName].Count > 1000)
                    {
                        _metricsBuffer[metricName] = _metricsBuffer[metricName].TakeLast(1000).ToList();
                    }
                }

                // Store in cache for recent metrics
                var cacheKey = $"metrics:{metricName}:{DateTime.UtcNow:yyyyMMddHH}";
                var hourlyMetrics = await _cachingService.GetAsync<List<object>>(cacheKey) ?? new List<object>();
                hourlyMetrics.Add(metric);
                await _cachingService.SetAsync(cacheKey, hourlyMetrics, TimeSpan.FromHours(24));

                // Check for threshold violations
                await CheckMetricThresholdsAsync(metricName, value, tags);

                _logger.LogDebug("Recorded metric {MetricName} = {Value}", metricName, value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording metric {MetricName}", metricName);
            }
        }

        /// <summary>
        /// Records performance counters for system resources.
        /// </summary>
        public async Task RecordPerformanceCounterAsync(PerformanceCounter counter)
        {
            try
            {
                await RecordMetricAsync(
                    $"{counter.Category}.{counter.Name}",
                    counter.Value,
                    counter.Tags.ToDictionary(kvp => kvp.Key, kvp => kvp.Value));

                // Store detailed counter information
                var cacheKey = $"performance_counters:{DateTime.UtcNow:yyyyMMddHH}";
                var hourlyCounters = await _cachingService.GetAsync<List<PerformanceCounter>>(cacheKey) ?? new List<PerformanceCounter>();
                hourlyCounters.Add(counter);

                // Keep only last 1000 counters per hour
                if (hourlyCounters.Count > 1000)
                {
                    hourlyCounters = hourlyCounters.TakeLast(1000).ToList();
                }

                await _cachingService.SetAsync(cacheKey, hourlyCounters, TimeSpan.FromHours(24));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording performance counter {Category}.{Name}", 
                    counter.Category, counter.Name);
            }
        }

        /// <summary>
        /// Logs structured application events with correlation IDs.
        /// </summary>
        public async Task LogStructuredEventAsync(StructuredLogEvent logEvent)
        {
            try
            {
                using var scope = _logger.BeginScope(new Dictionary<string, object>
                {
                    ["CorrelationId"] = logEvent.CorrelationId,
                    ["EventId"] = logEvent.EventId,
                    ["UserId"] = logEvent.UserId,
                    ["SessionId"] = logEvent.SessionId,
                    ["RequestId"] = logEvent.RequestId
                });

                // Log with appropriate level
                switch (logEvent.Level)
                {
                    case LogLevel.Critical:
                        _logger.LogCritical(logEvent.Exception, "{Message} {@Properties}", 
                            logEvent.Message, logEvent.Properties);
                        break;
                    case LogLevel.Error:
                        _logger.LogError(logEvent.Exception, "{Message} {@Properties}", 
                            logEvent.Message, logEvent.Properties);
                        break;
                    case LogLevel.Warning:
                        _logger.LogWarning("{Message} {@Properties}", logEvent.Message, logEvent.Properties);
                        break;
                    case LogLevel.Information:
                        _logger.LogInformation("{Message} {@Properties}", logEvent.Message, logEvent.Properties);
                        break;
                    case LogLevel.Debug:
                        _logger.LogDebug("{Message} {@Properties}", logEvent.Message, logEvent.Properties);
                        break;
                }

                // Store structured log for querying
                var cacheKey = $"structured_logs:{DateTime.UtcNow:yyyyMMdd}";
                var dailyLogs = await _cachingService.GetAsync<List<StructuredLogEvent>>(cacheKey) ?? new List<StructuredLogEvent>();
                dailyLogs.Add(logEvent);

                // Keep only recent logs in memory
                if (dailyLogs.Count > 10000)
                {
                    dailyLogs = dailyLogs.TakeLast(10000).ToList();
                }

                await _cachingService.SetAsync(cacheKey, dailyLogs, TimeSpan.FromDays(7));

                // Record error if applicable
                if (logEvent.Level >= LogLevel.Error && logEvent.Exception != null)
                {
                    await RecordErrorAsync(new ApplicationError
                    {
                        Message = logEvent.Message,
                        StackTrace = logEvent.Exception.StackTrace,
                        Source = logEvent.Source,
                        InnerException = logEvent.Exception.InnerException?.Message,
                        Severity = MapLogLevelToErrorSeverity(logEvent.Level),
                        UserId = logEvent.UserId,
                        SessionId = logEvent.SessionId,
                        Context = logEvent.Properties
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging structured event {EventId}", logEvent.EventId);
            }
        }

        /// <summary>
        /// Creates a performance monitoring scope for tracking operation duration.
        /// </summary>
        public IDisposable CreatePerformanceScope(string operationName, Dictionary<string, object>? properties = null)
        {
            return new PerformanceScope(this, operationName, properties ?? new Dictionary<string, object>());
        }

        /// <summary>
        /// Gets real-time system health status and metrics.
        /// </summary>
        public async Task<SystemHealthStatus> GetSystemHealthAsync()
        {
            try
            {
                var healthStatus = new SystemHealthStatus
                {
                    Version = GetType().Assembly.GetName().Version?.ToString() ?? "unknown",
                    Environment = _configuration["ASPNETCORE_ENVIRONMENT"] ?? "unknown",
                    Uptime = DateTime.UtcNow - _startTime
                };

                // Check database health
                healthStatus.Components["Database"] = await CheckDatabaseHealthAsync();
                
                // Check cache health
                healthStatus.Components["Cache"] = await CheckCacheHealthAsync();
                
                // Check external services
                healthStatus.Components["ExternalServices"] = await CheckExternalServicesHealthAsync();

                // Get system resources
                healthStatus.Resources = await GetSystemResourcesAsync();

                // Determine overall status
                var componentStatuses = healthStatus.Components.Values.Select(c => c.Status).ToList();
                if (componentStatuses.Any(s => s == "Critical"))
                    healthStatus.OverallStatus = "Critical";
                else if (componentStatuses.Any(s => s == "Unhealthy"))
                    healthStatus.OverallStatus = "Unhealthy";
                else if (componentStatuses.Any(s => s == "Degraded"))
                    healthStatus.OverallStatus = "Degraded";
                else
                    healthStatus.OverallStatus = "Healthy";

                return healthStatus;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting system health");
                return new SystemHealthStatus
                {
                    OverallStatus = "Critical",
                    Components = { ["System"] = new ComponentHealth 
                    { 
                        Name = "System", 
                        Status = "Critical", 
                        StatusMessage = "Health check failed: " + ex.Message 
                    }}
                };
            }
        }

        /// <summary>
        /// Gets application performance metrics for a given time range.
        /// </summary>
        public async Task<PerformanceMetrics> GetPerformanceMetricsAsync(DateTime fromDate, DateTime toDate)
        {
            try
            {
                var metrics = new PerformanceMetrics
                {
                    PeriodStart = fromDate,
                    PeriodEnd = toDate
                };

                // Aggregate metrics from cache
                var timeSpan = toDate - fromDate;
                var hours = (int)Math.Ceiling(timeSpan.TotalHours);
                
                var allMetrics = new Dictionary<string, List<double>>();
                
                for (int i = 0; i < hours; i++)
                {
                    var hour = fromDate.AddHours(i);
                    var cacheKey = $"metrics:*:{hour:yyyyMMddHH}";
                    
                    // In a real implementation, you'd query all matching keys
                    // For now, we'll use the buffered metrics
                    foreach (var metricName in _metricsBuffer.Keys)
                    {
                        if (!allMetrics.ContainsKey(metricName))
                            allMetrics[metricName] = new List<double>();
                        
                        allMetrics[metricName].AddRange(_metricsBuffer[metricName]);
                    }
                }

                // Calculate metric summaries
                foreach (var metricGroup in allMetrics)
                {
                    var values = metricGroup.Value;
                    if (values.Any())
                    {
                        metrics.Metrics[metricGroup.Key] = new MetricSummary
                        {
                            MetricName = metricGroup.Key,
                            Min = values.Min(),
                            Max = values.Max(),
                            Average = values.Average(),
                            Sum = values.Sum(),
                            Count = values.Count,
                            StandardDeviation = CalculateStandardDeviation(values),
                            Median = CalculateMedian(values)
                        };
                    }
                }

                // Calculate response time metrics from request duration metrics
                if (allMetrics.ContainsKey("request.duration"))
                {
                    var durations = allMetrics["request.duration"].OrderBy(x => x).ToList();
                    metrics.AverageResponseTime = durations.Average();
                    metrics.P95ResponseTime = CalculatePercentile(durations, 95);
                    metrics.P99ResponseTime = CalculatePercentile(durations, 99);
                }

                if (allMetrics.ContainsKey("request.count"))
                {
                    metrics.TotalRequests = (long)allMetrics["request.count"].Sum();
                    metrics.ThroughputPerSecond = metrics.TotalRequests / timeSpan.TotalSeconds;
                }

                if (allMetrics.ContainsKey("request.errors"))
                {
                    var errorCount = allMetrics["request.errors"].Sum();
                    metrics.ErrorRate = metrics.TotalRequests > 0 ? (errorCount / metrics.TotalRequests) * 100 : 0;
                }

                return metrics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting performance metrics from {FromDate} to {ToDate}", fromDate, toDate);
                return new PerformanceMetrics { PeriodStart = fromDate, PeriodEnd = toDate };
            }
        }

        /// <summary>
        /// Triggers alerts based on threshold violations or system anomalies.
        /// </summary>
        public async Task TriggerAlertAsync(MonitoringAlert alert)
        {
            try
            {
                // Add to active alerts
                lock (_activeAlerts)
                {
                    _activeAlerts.Add(alert);
                }

                // Store in cache
                var cacheKey = $"alerts:{alert.AlertId}";
                await _cachingService.SetAsync(cacheKey, alert, TimeSpan.FromDays(30));

                // Log the alert
                _logger.LogWarning("Alert triggered: {AlertName} | Severity: {Severity} | Value: {CurrentValue} | Threshold: {ThresholdValue}",
                    alert.AlertName, alert.Severity, alert.CurrentValue, alert.ThresholdValue);

                // In production, you would send notifications via email, Slack, PagerDuty, etc.
                await SendAlertNotificationAsync(alert);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error triggering alert {AlertId}", alert.AlertId);
            }
        }

        /// <summary>
        /// Gets active alerts and their status.
        /// </summary>
        public async Task<List<MonitoringAlert>> GetActiveAlertsAsync()
        {
            try
            {
                lock (_activeAlerts)
                {
                    return _activeAlerts.Where(a => a.Status == AlertStatus.Active).ToList();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active alerts");
                return new List<MonitoringAlert>();
            }
        }

        /// <summary>
        /// Acknowledges an alert to stop further notifications.
        /// </summary>
        public async Task AcknowledgeAlertAsync(string alertId, string acknowledgedBy, string notes = "")
        {
            try
            {
                lock (_activeAlerts)
                {
                    var alert = _activeAlerts.FirstOrDefault(a => a.AlertId == alertId);
                    if (alert != null)
                    {
                        alert.Status = AlertStatus.Acknowledged;
                        alert.AcknowledgedAt = DateTime.UtcNow;
                        alert.AcknowledgedBy = acknowledgedBy;
                        alert.Metadata["AcknowledgementNotes"] = notes;
                    }
                }

                // Update in cache
                var cacheKey = $"alerts:{alertId}";
                var cachedAlert = await _cachingService.GetAsync<MonitoringAlert>(cacheKey);
                if (cachedAlert != null)
                {
                    cachedAlert.Status = AlertStatus.Acknowledged;
                    cachedAlert.AcknowledgedAt = DateTime.UtcNow;
                    cachedAlert.AcknowledgedBy = acknowledgedBy;
                    await _cachingService.SetAsync(cacheKey, cachedAlert, TimeSpan.FromDays(30));
                }

                _logger.LogInformation("Alert {AlertId} acknowledged by {AcknowledgedBy}: {Notes}", 
                    alertId, acknowledgedBy, notes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error acknowledging alert {AlertId}", alertId);
            }
        }

        /// <summary>
        /// Records application errors with full context and stack traces.
        /// </summary>
        public async Task RecordErrorAsync(ApplicationError error)
        {
            try
            {
                // Store in cache for recent errors
                var cacheKey = $"errors:{DateTime.UtcNow:yyyyMMdd}";
                var dailyErrors = await _cachingService.GetAsync<List<ApplicationError>>(cacheKey) ?? new List<ApplicationError>();
                dailyErrors.Add(error);

                // Keep only recent errors in memory
                if (dailyErrors.Count > 1000)
                {
                    dailyErrors = dailyErrors.TakeLast(1000).ToList();
                }

                await _cachingService.SetAsync(cacheKey, dailyErrors, TimeSpan.FromDays(7));

                // Record error metrics
                await RecordMetricAsync("application.errors", 1, new Dictionary<string, string>
                {
                    ["severity"] = error.Severity.ToString(),
                    ["source"] = error.Source
                });

                // Trigger alert for critical errors
                if (error.Severity == ErrorSeverity.Critical)
                {
                    await TriggerAlertAsync(new MonitoringAlert
                    {
                        AlertName = "Critical Application Error",
                        Severity = MonitoringAlertSeverity.Critical,
                        Type = AlertType.Error,
                        Description = $"Critical error in {error.Source}: {error.Message}",
                        Source = error.Source,
                        Metadata = error.Context.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
                    });
                }

                _logger.LogError("Application error recorded: {ErrorId} | {Severity} | {Source} | {Message}",
                    error.ErrorId, error.Severity, error.Source, error.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording application error {ErrorId}", error.ErrorId);
            }
        }

        /// <summary>
        /// Gets error statistics and trends for monitoring dashboards.
        /// </summary>
        public async Task<ErrorStatistics> GetErrorStatisticsAsync(DateTime fromDate, DateTime toDate)
        {
            try
            {
                var statistics = new ErrorStatistics
                {
                    PeriodStart = fromDate,
                    PeriodEnd = toDate
                };

                // Aggregate errors from cache
                var days = (int)(toDate - fromDate).TotalDays + 1;
                var allErrors = new List<ApplicationError>();

                for (int i = 0; i < days; i++)
                {
                    var date = fromDate.AddDays(i);
                    var cacheKey = $"errors:{date:yyyyMMdd}";
                    var dayErrors = await _cachingService.GetAsync<List<ApplicationError>>(cacheKey) ?? new List<ApplicationError>();
                    allErrors.AddRange(dayErrors.Where(e => e.Timestamp >= fromDate && e.Timestamp <= toDate));
                }

                // Calculate statistics
                statistics.TotalErrors = allErrors.Count;
                statistics.UniqueErrors = allErrors.GroupBy(e => e.Message).Count();
                
                // Error rate (would need total requests for accurate calculation)
                statistics.ErrorRate = 0; // Simplified

                // Errors by severity
                statistics.ErrorsBySeverity = allErrors
                    .GroupBy(e => e.Severity)
                    .ToDictionary(g => g.Key, g => (long)g.Count());

                // Errors by source
                statistics.ErrorsBySource = allErrors
                    .GroupBy(e => e.Source)
                    .ToDictionary(g => g.Key, g => (long)g.Count());

                // Top errors
                statistics.TopErrors = allErrors
                    .GroupBy(e => e.Message)
                    .Select(g => new TopError
                    {
                        ErrorMessage = g.Key,
                        Count = g.Count(),
                        Severity = g.First().Severity,
                        FirstOccurrence = g.Min(e => e.Timestamp),
                        LastOccurrence = g.Max(e => e.Timestamp),
                        Source = g.First().Source
                    })
                    .OrderByDescending(e => e.Count)
                    .Take(10)
                    .ToList();

                return statistics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting error statistics from {FromDate} to {ToDate}", fromDate, toDate);
                return new ErrorStatistics { PeriodStart = fromDate, PeriodEnd = toDate };
            }
        }

        /// <summary>
        /// Tracks user activity and business events for analytics.
        /// </summary>
        public async Task TrackUserActivityAsync(UserActivity activity)
        {
            try
            {
                // Store in cache
                var cacheKey = $"user_activity:{DateTime.UtcNow:yyyyMMdd}";
                var dailyActivities = await _cachingService.GetAsync<List<UserActivity>>(cacheKey) ?? new List<UserActivity>();
                dailyActivities.Add(activity);

                if (dailyActivities.Count > 10000)
                {
                    dailyActivities = dailyActivities.TakeLast(10000).ToList();
                }

                await _cachingService.SetAsync(cacheKey, dailyActivities, TimeSpan.FromDays(30));

                // Record activity metrics
                await RecordMetricAsync("user.activity", 1, new Dictionary<string, string>
                {
                    ["type"] = activity.ActivityType,
                    ["action"] = activity.Action,
                    ["successful"] = activity.IsSuccessful.ToString()
                });

                _logger.LogDebug("User activity tracked: {UserId} | {ActivityType} | {Action} | {Resource}",
                    activity.UserId, activity.ActivityType, activity.Action, activity.Resource);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error tracking user activity for user {UserId}", activity.UserId);
            }
        }

        /// <summary>
        /// Gets user activity analytics and usage patterns.
        /// </summary>
        public async Task<UserActivityAnalytics> GetUserActivityAnalyticsAsync(DateTime fromDate, DateTime toDate)
        {
            try
            {
                var analytics = new UserActivityAnalytics
                {
                    PeriodStart = fromDate,
                    PeriodEnd = toDate
                };

                // Aggregate activities from cache
                var days = (int)(toDate - fromDate).TotalDays + 1;
                var allActivities = new List<UserActivity>();

                for (int i = 0; i < days; i++)
                {
                    var date = fromDate.AddDays(i);
                    var cacheKey = $"user_activity:{date:yyyyMMdd}";
                    var dayActivities = await _cachingService.GetAsync<List<UserActivity>>(cacheKey) ?? new List<UserActivity>();
                    allActivities.AddRange(dayActivities.Where(a => a.Timestamp >= fromDate && a.Timestamp <= toDate));
                }

                // Calculate analytics
                analytics.TotalActivities = allActivities.Count;
                analytics.UniqueUsers = allActivities.Select(a => a.UserId).Distinct().Count();
                analytics.TotalSessions = allActivities.Select(a => a.SessionId).Distinct().Count();
                
                if (analytics.TotalSessions > 0)
                {
                    var sessionDurations = allActivities
                        .GroupBy(a => a.SessionId)
                        .Select(g => g.Max(a => a.Timestamp) - g.Min(a => a.Timestamp))
                        .Where(d => d.TotalMinutes > 0)
                        .ToList();
                    
                    analytics.AverageSessionDuration = sessionDurations.Any() 
                        ? TimeSpan.FromTicks((long)sessionDurations.Average(d => d.Ticks))
                        : TimeSpan.Zero;
                }

                analytics.ActivitiesByType = allActivities
                    .GroupBy(a => a.ActivityType)
                    .ToDictionary(g => g.Key, g => (long)g.Count());

                analytics.PopularResources = allActivities
                    .GroupBy(a => a.Resource)
                    .ToDictionary(g => g.Key, g => (long)g.Count());

                return analytics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user activity analytics from {FromDate} to {ToDate}", fromDate, toDate);
                return new UserActivityAnalytics { PeriodStart = fromDate, PeriodEnd = toDate };
            }
        }

        /// <summary>
        /// Exports monitoring data to external systems.
        /// </summary>
        public async Task<string> ExportMetricsAsync(MetricsExportFormat format, DateTime fromDate, DateTime toDate)
        {
            try
            {
                var performanceMetrics = await GetPerformanceMetricsAsync(fromDate, toDate);

                return format switch
                {
                    MetricsExportFormat.Json => JsonSerializer.Serialize(performanceMetrics, new JsonSerializerOptions { WriteIndented = true }),
                    MetricsExportFormat.Csv => ExportToCsv(performanceMetrics),
                    MetricsExportFormat.Prometheus => ExportToPrometheus(performanceMetrics),
                    _ => JsonSerializer.Serialize(performanceMetrics)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting metrics in format {Format}", format);
                return string.Empty;
            }
        }

        #region Private Helper Methods

        private async Task CheckMetricThresholdsAsync(string metricName, double value, Dictionary<string, string>? tags)
        {
            // Define threshold rules (would be configurable in production)
            var thresholds = new Dictionary<string, (double warning, double critical)>
            {
                ["cpu.usage"] = (80.0, 95.0),
                ["memory.usage"] = (85.0, 95.0),
                ["response.time"] = (1000.0, 5000.0), // milliseconds
                ["error.rate"] = (5.0, 10.0), // percentage
                ["disk.usage"] = (85.0, 95.0)
            };

            if (thresholds.TryGetValue(metricName, out var threshold))
            {
                if (value >= threshold.critical)
                {
                    await TriggerAlertAsync(new MonitoringAlert
                    {
                        AlertName = $"Critical {metricName}",
                        Severity = MonitoringAlertSeverity.Critical,
                        Type = AlertType.Performance,
                        Description = $"{metricName} has reached critical level",
                        Condition = $"{metricName} >= {threshold.critical}",
                        CurrentValue = value,
                        ThresholdValue = threshold.critical,
                        Source = "MetricThreshold"
                    });
                }
                else if (value >= threshold.warning)
                {
                    await TriggerAlertAsync(new MonitoringAlert
                    {
                        AlertName = $"Warning {metricName}",
                        Severity = MonitoringAlertSeverity.Warning,
                        Type = AlertType.Performance,
                        Description = $"{metricName} has reached warning level",
                        Condition = $"{metricName} >= {threshold.warning}",
                        CurrentValue = value,
                        ThresholdValue = threshold.warning,
                        Source = "MetricThreshold"
                    });
                }
            }
        }

        private async Task<ComponentHealth> CheckDatabaseHealthAsync()
        {
            try
            {
                var stopwatch = Stopwatch.StartNew();
                // Would perform actual database health check
                await Task.Delay(10); // Simulate check
                stopwatch.Stop();

                return new ComponentHealth
                {
                    ComponentId = Guid.NewGuid().ToString(),
                    Name = "Database",
                    Type = "Database",
                    Status = "Healthy",
                    HealthScore = 1.0,
                    Metrics = new List<HealthMetric>(),
                    LastChecked = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                return new ComponentHealth
                {
                    ComponentId = Guid.NewGuid().ToString(),
                    Name = "Database",
                    Type = "Database",
                    Status = "Unhealthy",
                    HealthScore = 0.0,
                    Metrics = new List<HealthMetric>(),
                    LastChecked = DateTime.UtcNow
                };
            }
        }

        private async Task<ComponentHealth> CheckCacheHealthAsync()
        {
            try
            {
                var stopwatch = Stopwatch.StartNew();
                await _cachingService.SetAsync("health_check", "test", TimeSpan.FromSeconds(10));
                var result = await _cachingService.GetAsync<string>("health_check");
                stopwatch.Stop();

                return new ComponentHealth
                {
                    ComponentId = Guid.NewGuid().ToString(),
                    Name = "Cache",
                    Type = "Cache",
                    Status = result == "test" ? "Healthy" : "Degraded",
                    HealthScore = result == "test" ? 1.0 : 0.5,
                    Metrics = new List<HealthMetric>(),
                    LastChecked = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                return new ComponentHealth
                {
                    ComponentId = Guid.NewGuid().ToString(),
                    Name = "Cache",
                    Type = "Cache",
                    Status = "Unhealthy",
                    HealthScore = 0.0,
                    Metrics = new List<HealthMetric>(),
                    LastChecked = DateTime.UtcNow
                };
            }
        }

        private async Task<ComponentHealth> CheckExternalServicesHealthAsync()
        {
            // Placeholder for external service health checks
            return await Task.FromResult(new ComponentHealth
            {
                ComponentId = Guid.NewGuid().ToString(),
                Name = "ExternalServices",
                Type = "ExternalService",
                Status = "Healthy",
                HealthScore = 1.0,
                Metrics = new List<HealthMetric>(),
                LastChecked = DateTime.UtcNow
            });
        }

        private async Task<SystemResources> GetSystemResourcesAsync()
        {
            try
            {
                var process = Process.GetCurrentProcess();
                var totalMemory = GC.GetTotalMemory(false);

                return new SystemResources
                {
                    CpuUsagePercentage = 0, // Would use performance counters
                    MemoryUsagePercentage = 0, // Would calculate based on system memory
                    AvailableMemoryMB = (totalMemory / 1024 / 1024),
                    TotalMemoryMB = Environment.WorkingSet / 1024 / 1024,
                    ThreadCount = process.Threads.Count,
                    ActiveConnections = 0 // Would get from connection pool
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting system resources");
                return new SystemResources();
            }
        }

        private async Task SendAlertNotificationAsync(MonitoringAlert alert)
        {
            // In production, implement actual notification sending
            // via email, Slack, Teams, PagerDuty, etc.
            await Task.CompletedTask;
        }

        private static ErrorSeverity MapLogLevelToErrorSeverity(LogLevel logLevel)
        {
            return logLevel switch
            {
                LogLevel.Critical => ErrorSeverity.Critical,
                LogLevel.Error => ErrorSeverity.High,
                LogLevel.Warning => ErrorSeverity.Medium,
                _ => ErrorSeverity.Low
            };
        }

        private static double CalculateStandardDeviation(List<double> values)
        {
            if (values.Count <= 1) return 0;
            
            var average = values.Average();
            var sumOfSquares = values.Sum(v => Math.Pow(v - average, 2));
            return Math.Sqrt(sumOfSquares / (values.Count - 1));
        }

        private static double CalculateMedian(List<double> values)
        {
            var sorted = values.OrderBy(x => x).ToList();
            var count = sorted.Count;
            
            if (count % 2 == 0)
            {
                return (sorted[count / 2 - 1] + sorted[count / 2]) / 2.0;
            }
            else
            {
                return sorted[count / 2];
            }
        }

        private static double CalculatePercentile(List<double> values, int percentile)
        {
            if (values.Count == 0) return 0;
            
            var index = (percentile / 100.0) * (values.Count - 1);
            
            if (index == (int)index)
            {
                return values[(int)index];
            }
            else
            {
                var lower = (int)Math.Floor(index);
                var upper = (int)Math.Ceiling(index);
                var weight = index - lower;
                
                return values[lower] * (1 - weight) + values[upper] * weight;
            }
        }

        private static string ExportToCsv(PerformanceMetrics metrics)
        {
            var csv = new System.Text.StringBuilder();
            csv.AppendLine("MetricName,Min,Max,Average,Sum,Count");
            
            foreach (var metric in metrics.Metrics)
            {
                csv.AppendLine($"{metric.Key},{metric.Value.Min},{metric.Value.Max},{metric.Value.Average},{metric.Value.Sum},{metric.Value.Count}");
            }
            
            return csv.ToString();
        }

        private static string ExportToPrometheus(PerformanceMetrics metrics)
        {
            var prometheus = new System.Text.StringBuilder();
            
            foreach (var metric in metrics.Metrics)
            {
                var name = metric.Key.Replace(".", "_").Replace("-", "_");
                prometheus.AppendLine($"# TYPE {name} gauge");
                prometheus.AppendLine($"{name}_min {metric.Value.Min}");
                prometheus.AppendLine($"{name}_max {metric.Value.Max}");
                prometheus.AppendLine($"{name}_avg {metric.Value.Average}");
            }
            
            return prometheus.ToString();
        }

        #endregion
    }

    /// <summary>
    /// Performance monitoring scope for automatic duration tracking.
    /// </summary>
    public class PerformanceScope : IDisposable
    {
        private readonly MonitoringService _monitoringService;
        private readonly string _operationName;
        private readonly Dictionary<string, object> _properties;
        private readonly Stopwatch _stopwatch;
        private bool _disposed;

        public PerformanceScope(MonitoringService monitoringService, string operationName, Dictionary<string, object> properties)
        {
            _monitoringService = monitoringService;
            _operationName = operationName;
            _properties = properties;
            _stopwatch = Stopwatch.StartNew();
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _stopwatch.Stop();
                
                // Record the operation duration
                Task.Run(async () =>
                {
                    await _monitoringService.RecordMetricAsync(
                        $"operation.duration.{_operationName.ToLowerInvariant().Replace(" ", "_")}",
                        _stopwatch.ElapsedMilliseconds,
                        _properties.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToString()));
                });

                _disposed = true;
            }
        }
    }
}