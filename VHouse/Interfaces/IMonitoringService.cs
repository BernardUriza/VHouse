using VHouse.Classes;

namespace VHouse.Interfaces
{
    /// <summary>
    /// Service interface for production monitoring, logging, and observability.
    /// </summary>
    public interface IMonitoringService
    {
        /// <summary>
        /// Records application metrics for monitoring and alerting.
        /// </summary>
        Task RecordMetricAsync(string metricName, double value, Dictionary<string, string>? tags = null);

        /// <summary>
        /// Records performance counters for system resources.
        /// </summary>
        Task RecordPerformanceCounterAsync(PerformanceCounter counter);

        /// <summary>
        /// Logs structured application events with correlation IDs.
        /// </summary>
        Task LogStructuredEventAsync(StructuredLogEvent logEvent);

        /// <summary>
        /// Creates a performance monitoring scope for tracking operation duration.
        /// </summary>
        IDisposable CreatePerformanceScope(string operationName, Dictionary<string, object>? properties = null);

        /// <summary>
        /// Gets real-time system health status and metrics.
        /// </summary>
        Task<SystemHealthStatus> GetSystemHealthAsync();

        /// <summary>
        /// Gets application performance metrics for a given time range.
        /// </summary>
        Task<PerformanceMetrics> GetPerformanceMetricsAsync(DateTime fromDate, DateTime toDate);

        /// <summary>
        /// Triggers alerts based on threshold violations or system anomalies.
        /// </summary>
        Task TriggerAlertAsync(MonitoringAlert alert);

        /// <summary>
        /// Gets active alerts and their status.
        /// </summary>
        Task<List<MonitoringAlert>> GetActiveAlertsAsync();

        /// <summary>
        /// Acknowledges an alert to stop further notifications.
        /// </summary>
        Task AcknowledgeAlertAsync(string alertId, string acknowledgedBy, string notes = "");

        /// <summary>
        /// Records application errors with full context and stack traces.
        /// </summary>
        Task RecordErrorAsync(ApplicationError error);

        /// <summary>
        /// Gets error statistics and trends for monitoring dashboards.
        /// </summary>
        Task<ErrorStatistics> GetErrorStatisticsAsync(DateTime fromDate, DateTime toDate);

        /// <summary>
        /// Tracks user activity and business events for analytics.
        /// </summary>
        Task TrackUserActivityAsync(UserActivity activity);

        /// <summary>
        /// Gets user activity analytics and usage patterns.
        /// </summary>
        Task<UserActivityAnalytics> GetUserActivityAnalyticsAsync(DateTime fromDate, DateTime toDate);

        /// <summary>
        /// Exports monitoring data to external systems (Prometheus, Grafana, etc.).
        /// </summary>
        Task<string> ExportMetricsAsync(MetricsExportFormat format, DateTime fromDate, DateTime toDate);
    }

    /// <summary>
    /// Performance counter data structure.
    /// </summary>
    public class PerformanceCounter
    {
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public double Value { get; set; }
        public string Unit { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public Dictionary<string, string> Tags { get; set; } = new();
        public string InstanceName { get; set; } = Environment.MachineName;
    }

    /// <summary>
    /// Structured logging event with correlation tracking.
    /// </summary>
    public class StructuredLogEvent
    {
        public string EventId { get; set; } = Guid.NewGuid().ToString();
        public string CorrelationId { get; set; } = string.Empty;
        public LogLevel Level { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public Dictionary<string, object> Properties { get; set; } = new();
        public Exception? Exception { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string SessionId { get; set; } = string.Empty;
        public string RequestId { get; set; } = string.Empty;
    }

    /// <summary>
    /// System health status information.
    /// </summary>
    public class SystemHealthStatus
    {
        public HealthStatus OverallStatus { get; set; } = HealthStatus.Healthy;
        public Dictionary<string, ComponentHealth> Components { get; set; } = new();
        public SystemResources Resources { get; set; } = new();
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
        public TimeSpan Uptime { get; set; }
        public string Version { get; set; } = string.Empty;
        public string Environment { get; set; } = string.Empty;
    }

    /// <summary>
    /// Health status levels.
    /// </summary>
    public enum HealthStatus
    {
        Healthy,
        Degraded,
        Unhealthy,
        Critical
    }

    /// <summary>
    /// Component health information.
    /// </summary>
    public class ComponentHealth
    {
        public string Name { get; set; } = string.Empty;
        public HealthStatus Status { get; set; } = HealthStatus.Healthy;
        public string StatusMessage { get; set; } = string.Empty;
        public TimeSpan ResponseTime { get; set; }
        public DateTime LastChecked { get; set; } = DateTime.UtcNow;
        public Dictionary<string, object> Details { get; set; } = new();
        public List<string> Dependencies { get; set; } = new();
    }

    /// <summary>
    /// System resource utilization.
    /// </summary>
    public class SystemResources
    {
        public double CpuUsagePercentage { get; set; }
        public double MemoryUsagePercentage { get; set; }
        public long AvailableMemoryMB { get; set; }
        public long TotalMemoryMB { get; set; }
        public double DiskUsagePercentage { get; set; }
        public long AvailableDiskSpaceGB { get; set; }
        public int ActiveConnections { get; set; }
        public int ThreadCount { get; set; }
        public double NetworkThroughputMbps { get; set; }
    }

    /// <summary>
    /// Performance metrics aggregation.
    /// </summary>
    public class PerformanceMetrics
    {
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public Dictionary<string, MetricSummary> Metrics { get; set; } = new();
        public List<PerformanceTrend> Trends { get; set; } = new();
        public double AverageResponseTime { get; set; }
        public double P95ResponseTime { get; set; }
        public double P99ResponseTime { get; set; }
        public long TotalRequests { get; set; }
        public double ThroughputPerSecond { get; set; }
        public double ErrorRate { get; set; }
    }

    /// <summary>
    /// Metric summary statistics.
    /// </summary>
    public class MetricSummary
    {
        public string MetricName { get; set; } = string.Empty;
        public double Min { get; set; }
        public double Max { get; set; }
        public double Average { get; set; }
        public double Sum { get; set; }
        public long Count { get; set; }
        public double StandardDeviation { get; set; }
        public double Median { get; set; }
        public string Unit { get; set; } = string.Empty;
    }

    /// <summary>
    /// Performance trend data point.
    /// </summary>
    public class PerformanceTrend
    {
        public DateTime Timestamp { get; set; }
        public string MetricName { get; set; } = string.Empty;
        public double Value { get; set; }
        public TrendDirection Direction { get; set; }
        public double ChangePercentage { get; set; }
    }

    /// <summary>
    /// Trend direction enumeration.
    /// </summary>
    public enum TrendDirection
    {
        Stable,
        Increasing,
        Decreasing,
        Volatile
    }

    /// <summary>
    /// Monitoring alert information.
    /// </summary>
    public class MonitoringAlert
    {
        public string AlertId { get; set; } = Guid.NewGuid().ToString();
        public string AlertName { get; set; } = string.Empty;
        public AlertSeverity Severity { get; set; }
        public AlertType Type { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Condition { get; set; } = string.Empty;
        public double CurrentValue { get; set; }
        public double ThresholdValue { get; set; }
        public DateTime TriggeredAt { get; set; } = DateTime.UtcNow;
        public DateTime? AcknowledgedAt { get; set; }
        public string? AcknowledgedBy { get; set; }
        public AlertStatus Status { get; set; } = AlertStatus.Active;
        public string Source { get; set; } = string.Empty;
        public Dictionary<string, object> Metadata { get; set; } = new();
        public List<string> NotificationChannels { get; set; } = new();
    }

    /// <summary>
    /// Alert severity levels.
    /// </summary>
    public enum AlertSeverity
    {
        Info,
        Warning,
        Error,
        Critical
    }

    /// <summary>
    /// Alert type categories.
    /// </summary>
    public enum AlertType
    {
        Performance,
        Error,
        Security,
        Business,
        Infrastructure
    }

    /// <summary>
    /// Alert status enumeration.
    /// </summary>
    public enum AlertStatus
    {
        Active,
        Acknowledged,
        Resolved,
        Suppressed
    }

    /// <summary>
    /// Application error information.
    /// </summary>
    public class ApplicationError
    {
        public string ErrorId { get; set; } = Guid.NewGuid().ToString();
        public string Message { get; set; } = string.Empty;
        public string? StackTrace { get; set; }
        public string Source { get; set; } = string.Empty;
        public string? InnerException { get; set; }
        public ErrorSeverity Severity { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string UserId { get; set; } = string.Empty;
        public string SessionId { get; set; } = string.Empty;
        public string RequestPath { get; set; } = string.Empty;
        public string UserAgent { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public Dictionary<string, object> Context { get; set; } = new();
        public bool IsHandled { get; set; }
        public int OccurrenceCount { get; set; } = 1;
    }

    /// <summary>
    /// Error severity levels.
    /// </summary>
    public enum ErrorSeverity
    {
        Low,
        Medium,
        High,
        Critical
    }

    /// <summary>
    /// Error statistics and trends.
    /// </summary>
    public class ErrorStatistics
    {
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public long TotalErrors { get; set; }
        public long UniqueErrors { get; set; }
        public double ErrorRate { get; set; }
        public Dictionary<ErrorSeverity, long> ErrorsBySeverity { get; set; } = new();
        public Dictionary<string, long> ErrorsBySource { get; set; } = new();
        public List<ErrorTrend> ErrorTrends { get; set; } = new();
        public List<TopError> TopErrors { get; set; } = new();
    }

    /// <summary>
    /// Error trend data point.
    /// </summary>
    public class ErrorTrend
    {
        public DateTime Timestamp { get; set; }
        public long ErrorCount { get; set; }
        public double ErrorRate { get; set; }
        public ErrorSeverity MostCommonSeverity { get; set; }
    }

    /// <summary>
    /// Top error occurrence.
    /// </summary>
    public class TopError
    {
        public string ErrorMessage { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public long Count { get; set; }
        public ErrorSeverity Severity { get; set; }
        public DateTime FirstOccurrence { get; set; }
        public DateTime LastOccurrence { get; set; }
    }

    /// <summary>
    /// User activity tracking.
    /// </summary>
    public class UserActivity
    {
        public string ActivityId { get; set; } = Guid.NewGuid().ToString();
        public string UserId { get; set; } = string.Empty;
        public string SessionId { get; set; } = string.Empty;
        public string ActivityType { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string Resource { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public TimeSpan Duration { get; set; }
        public string IpAddress { get; set; } = string.Empty;
        public string UserAgent { get; set; } = string.Empty;
        public Dictionary<string, object> Properties { get; set; } = new();
        public bool IsSuccessful { get; set; } = true;
        public string? ErrorMessage { get; set; }
    }

    /// <summary>
    /// User activity analytics.
    /// </summary>
    public class UserActivityAnalytics
    {
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public long TotalActivities { get; set; }
        public long UniqueUsers { get; set; }
        public long TotalSessions { get; set; }
        public TimeSpan AverageSessionDuration { get; set; }
        public Dictionary<string, long> ActivitiesByType { get; set; } = new();
        public Dictionary<string, long> PopularResources { get; set; } = new();
        public List<UserActivityTrend> ActivityTrends { get; set; } = new();
        public List<UserEngagementMetric> UserEngagement { get; set; } = new();
    }

    /// <summary>
    /// User activity trend.
    /// </summary>
    public class UserActivityTrend
    {
        public DateTime Timestamp { get; set; }
        public long ActiveUsers { get; set; }
        public long TotalActivities { get; set; }
        public TimeSpan AverageSessionLength { get; set; }
        public double EngagementRate { get; set; }
    }

    /// <summary>
    /// User engagement metrics.
    /// </summary>
    public class UserEngagementMetric
    {
        public string UserId { get; set; } = string.Empty;
        public long TotalActivities { get; set; }
        public TimeSpan TotalTimeSpent { get; set; }
        public int SessionCount { get; set; }
        public DateTime FirstActivity { get; set; }
        public DateTime LastActivity { get; set; }
        public double EngagementScore { get; set; }
        public List<string> TopActions { get; set; } = new();
    }

    /// <summary>
    /// Metrics export formats.
    /// </summary>
    public enum MetricsExportFormat
    {
        Json,
        Csv,
        Prometheus,
        InfluxDB,
        Grafana
    }
}