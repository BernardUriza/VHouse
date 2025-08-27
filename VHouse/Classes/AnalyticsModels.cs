using System.ComponentModel.DataAnnotations;

namespace VHouse.Classes;

// Analytics models to resolve compilation errors
public class LiveMetrics
{
    public string MetricCategory { get; set; } = string.Empty;
    public Dictionary<string, double> Metrics { get; set; } = new();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    public string Category { get; set; } = string.Empty;
    public Dictionary<string, double> Values { get; set; } = new();
}

public class PerformanceAnalytics
{
    public string AnalyticsId { get; set; } = string.Empty;
    public TimeSpan Period { get; set; }
    public Dictionary<string, object> Metrics { get; set; } = new();
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

public class PredictionParameters
{
    public string ModelType { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public Dictionary<string, object> Features { get; set; } = new();
}

public class TrendQuery
{
    public string DataType { get; set; } = string.Empty;
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public string Granularity { get; set; } = "daily";
    
    public string MetricName { get; set; } = string.Empty;
    public TimeSpan WindowSize { get; set; } = TimeSpan.FromDays(30);
    public bool AnalyzeSeasonality { get; set; } = false;
}

public class TrendAnalysis
{
    public string AnalysisId { get; set; } = string.Empty;
    public List<TrendPoint> TrendPoints { get; set; } = new();
    public string TrendDirection { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

public class TrendPoint
{
    public DateTime Timestamp { get; set; }
    public double Value { get; set; }
    public Dictionary<string, object> Context { get; set; } = new();
}

public class DatasetParameters
{
    public string DatasetId { get; set; } = string.Empty;
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public List<string> Features { get; set; } = new();
}

public class AnomalyDetectionResult
{
    public string DetectionId { get; set; } = string.Empty;
    public List<Anomaly> Anomalies { get; set; } = new();
    public DateTime DetectedAt { get; set; } = DateTime.UtcNow;
}

public class Anomaly
{
    public DateTime Timestamp { get; set; }
    public double AnomalyScore { get; set; }
    public Dictionary<string, object> Context { get; set; } = new();
}

public class InventoryOptimizationRequest
{
    public string ProductCategory { get; set; } = string.Empty;
    public DateTime ForecastPeriod { get; set; }
    public Dictionary<string, object> Constraints { get; set; } = new();
}

public class OptimizationResult
{
    public string OptimizationId { get; set; } = string.Empty;
    public Dictionary<string, object> Recommendations { get; set; } = new();
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

public class CustomerAnalyticsQuery
{
    public string SegmentId { get; set; } = string.Empty;
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public List<string> Metrics { get; set; } = new();
}

public class CustomerAnalytics
{
    public string AnalyticsId { get; set; } = string.Empty;
    public Dictionary<string, object> CustomerMetrics { get; set; } = new();
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

public class ProductAnalyticsQuery
{
    public string ProductId { get; set; } = string.Empty;
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public List<string> Metrics { get; set; } = new();
}

public class ProductAnalytics
{
    public string AnalyticsId { get; set; } = string.Empty;
    public string ProductId { get; set; } = string.Empty;
    public Dictionary<string, object> ProductMetrics { get; set; } = new();
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

public class RevenueQuery
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public List<string> RevenueStreams { get; set; } = new();
    public string Granularity { get; set; } = "daily";
}

public class RevenueAnalytics
{
    public string AnalyticsId { get; set; } = string.Empty;
    public Dictionary<string, decimal> RevenueMetrics { get; set; } = new();
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

public class BatchDataRequest
{
    public string DataSource { get; set; } = string.Empty;
    public string ProcessingType { get; set; } = string.Empty;
    public Dictionary<string, object> Parameters { get; set; } = new();
}

public class DataProcessingResult
{
    public string ProcessingId { get; set; } = string.Empty;
    public bool Success { get; set; }
    public Dictionary<string, object> Results { get; set; } = new();
    public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
}

public class AggregationQuery
{
    public string DataSource { get; set; } = string.Empty;
    public List<string> Dimensions { get; set; } = new();
    public List<string> Metrics { get; set; } = new();
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
}

public class AggregationResult
{
    public string AggregationId { get; set; } = string.Empty;
    public Dictionary<string, object> AggregatedData { get; set; } = new();
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

public class CorrelationParameters
{
    public List<string> Variables { get; set; } = new();
    public string CorrelationMethod { get; set; } = "pearson";
    public Dictionary<string, object> Settings { get; set; } = new();
}

public class CorrelationAnalysis
{
    public string AnalysisId { get; set; } = string.Empty;
    public Dictionary<string, double> Correlations { get; set; } = new();
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

public class AnalyticsSegmentationCriteria
{
    public List<string> Features { get; set; } = new();
    public int NumberOfSegments { get; set; }
    public string Method { get; set; } = "kmeans";
}

public class SegmentationResult
{
    public string SegmentationId { get; set; } = string.Empty;
    public List<DataSegment> Segments { get; set; } = new();
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
}

public class DataSegment
{
    public string SegmentId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Size { get; set; }
    public Dictionary<string, object> Characteristics { get; set; } = new();
}

public class ExportRequest
{
    public string DataType { get; set; } = string.Empty;
    public string Format { get; set; } = string.Empty;
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public Dictionary<string, object> Filters { get; set; } = new();
}

public class ExportResult
{
    public string ExportId { get; set; } = string.Empty;
    public string DownloadUrl { get; set; } = string.Empty;
    public string Format { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateTime ExportedAt { get; set; } = DateTime.UtcNow;
}

public class ReportSchedule
{
    public string ScheduleId { get; set; } = string.Empty;
    public string ReportType { get; set; } = string.Empty;
    public string CronExpression { get; set; } = string.Empty;
    public List<string> Recipients { get; set; } = new();
    public Dictionary<string, object> Parameters { get; set; } = new();
}

public class ScheduledReport
{
    public string ReportId { get; set; } = string.Empty;
    public string ScheduleId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public ReportSchedule Schedule { get; set; } = new();
}

public class ReportStatus
{
    public string ReportId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int Progress { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    
    public static string Scheduled { get; set; } = "Scheduled";
    public static string NotFound { get; set; } = "NotFound";
}

public class KeyPerformanceIndicator
{
    public string Name { get; set; } = string.Empty;
    public double Value { get; set; }
    public double Target { get; set; }
    public string Unit { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}

public class Trend
{
    public string Name { get; set; } = string.Empty;
    public string Direction { get; set; } = string.Empty;
    public double ChangePercentage { get; set; }
    public string Description { get; set; } = string.Empty;
}

public class BusinessOpportunity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double PotentialImpact { get; set; }
    public string Priority { get; set; } = string.Empty;
}

public class Risk
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Probability { get; set; }
    public double Impact { get; set; }
    public string MitigationPlan { get; set; } = string.Empty;
}

public class Insight
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    
    public string Type { get; set; } = string.Empty;
    public string Impact { get; set; } = string.Empty;
    public List<string> Recommendations { get; set; } = new();
}

public class Series
{
    public string Name { get; set; } = string.Empty;
    public List<DataPoint> Data { get; set; } = new();
    public string Type { get; set; } = string.Empty;
}

public class DataPoint
{
    public DateTime Timestamp { get; set; }
    public double Value { get; set; }
    public Dictionary<string, object> Context { get; set; } = new();
    
    public DateTime X { get; set; }
    public double Y { get; set; }
}

public class AxisConfiguration
{
    public string Title { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public double? Min { get; set; }
    public double? Max { get; set; }
}

// Additional missing analytics models
public class AnalyticsQuery
{
    public string QueryType { get; set; } = string.Empty;
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<string> Metrics { get; set; } = new();
    public Dictionary<string, object> Filters { get; set; } = new();
    public string Granularity { get; set; } = "hourly";
    public string ApiId { get; set; } = string.Empty;
}

public class RealtimeReport
{
    public string ReportId { get; set; } = string.Empty;
    public string ReportType { get; set; } = string.Empty;
    public Dictionary<string, object> Data { get; set; } = new();
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public bool IsRealtime { get; set; } = true;
}

public class Pattern
{
    public string PatternId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public Dictionary<string, object> Parameters { get; set; } = new();
    public DateTime DetectedAt { get; set; } = DateTime.UtcNow;
}

// Additional API Gateway models
public class APIVersionRequest
{
    public string ApiId { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public bool BackwardCompatible { get; set; }
    public DateTime? DeprecationDate { get; set; }
    public List<string> ChangeLog { get; set; } = new();
    public string APIName { get; set; } = string.Empty;
}

public class VersioningResult
{
    public bool Success { get; set; }
    public string VersionId { get; set; } = string.Empty;
    public string ApiId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? DeprecationDate { get; set; }
    public bool BackwardCompatible { get; set; }
    public string MigrationPath { get; set; } = string.Empty;
    public List<string> ChangeLog { get; set; } = new();
}

public class APIValidationRequest
{
    public string APIEndpoint { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public Dictionary<string, object> Headers { get; set; } = new();
    public object RequestBody { get; set; } = new();
    public List<string> ValidationRules { get; set; } = new();
}

// Seasonality Analysis Models
public class SeasonalityAnalysis
{
    public bool HasSeasonality { get; set; }
    public string Period { get; set; } = string.Empty;
    public double SeasonalStrength { get; set; }
    public Dictionary<string, double> SeasonalFactors { get; set; } = new();
    public List<SeasonalPattern> Patterns { get; set; } = new();
    public double ConfidenceLevel { get; set; }
}

public class SeasonalPattern
{
    public string PatternType { get; set; } = string.Empty;
    public int PeriodLength { get; set; }
    public double Amplitude { get; set; }
    public double PhaseShift { get; set; }
}

public enum TrendDirection
{
    Up,
    Down,
    Stable
}

// Analytics Core Models
public class AnalyticsReport
{
    public string ReportId { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }
    public Dictionary<string, object> Data { get; set; } = new();
    public List<ChartData> Charts { get; set; } = new();
    public List<TableData> Tables { get; set; } = new();
    public Dictionary<string, double> Summary { get; set; } = new();
    public List<Insight> Insights { get; set; } = new();
    public TimeSpan ProcessingTime { get; set; }
}

public class ChartData
{
    public string ChartType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public List<Series> Series { get; set; } = new();
    public AxisConfiguration XAxis { get; set; } = new();
    public AxisConfiguration YAxis { get; set; } = new();
}

public class TableData
{
    public string Title { get; set; } = string.Empty;
    public List<string> Columns { get; set; } = new();
    public List<Dictionary<string, object>> Rows { get; set; } = new();
    public Dictionary<string, object> Totals { get; set; } = new();
}


public class StreamAnalyticsResult
{
    public string StreamId { get; set; } = string.Empty;
    public DateTime ProcessedAt { get; set; }
    public Dictionary<string, object> Results { get; set; } = new();
    public List<string> TriggeredAlerts { get; set; } = new();
}

public class BusinessEvent
{
    public string EventId { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public Dictionary<string, object> Data { get; set; } = new();
    public string Source { get; set; } = string.Empty;
    public Dictionary<string, object> Payload { get; set; } = new();
}

public class ForecastResult
{
    public string PredictionId { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public Dictionary<string, object> ForecastData { get; set; } = new();
    public DateTime GeneratedAt { get; set; }
    public List<PredictionPoint> Points { get; set; } = new();
}

public class PredictionPoint
{
    public DateTime Date { get; set; }
    public double Value { get; set; }
    public double LowerBound { get; set; }
    public double UpperBound { get; set; }
    public double Confidence { get; set; }
}

public class BusinessInsights
{
    public List<KeyPerformanceIndicator> KPIs { get; set; } = new();
    public List<Trend> Trends { get; set; } = new();
    public List<BusinessOpportunity> Opportunities { get; set; } = new();
    public List<Risk> Risks { get; set; } = new();
    public Dictionary<string, object> ExecutiveSummary { get; set; } = new();
}

