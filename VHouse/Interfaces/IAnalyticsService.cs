using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VHouse.Models;

namespace VHouse.Interfaces
{
    public interface IAnalyticsService
    {
        // Real-time Analytics
        Task<AnalyticsReport> GenerateRealtimeReportAsync(AnalyticsQuery query);
        Task<StreamAnalyticsResult> ProcessEventStreamAsync(BusinessEvent businessEvent);
        Task<LiveMetrics> GetLiveMetricsAsync(string metricCategory);
        Task<PerformanceAnalytics> AnalyzeSystemPerformanceAsync(TimeSpan period);
        
        // Predictive Analytics
        Task<ForecastResult> PredictDemandAsync(PredictionParameters parameters);
        Task<TrendAnalysis> AnalyzeTrendsAsync(TrendQuery query);
        Task<AnomalyDetectionResult> DetectAnomaliesAsync(DatasetParameters parameters);
        Task<OptimizationResult> OptimizeInventoryAsync(InventoryOptimizationRequest request);
        
        // Business Insights
        Task<BusinessInsights> GetBusinessInsightsAsync(DateTime fromDate, DateTime toDate);
        Task<CustomerAnalytics> AnalyzeCustomerBehaviorAsync(CustomerAnalyticsQuery query);
        Task<ProductAnalytics> AnalyzeProductPerformanceAsync(ProductAnalyticsQuery query);
        Task<RevenueAnalytics> AnalyzeRevenueStreamsAsync(RevenueQuery query);
        
        // Data Processing
        Task<DataProcessingResult> ProcessBatchDataAsync(BatchDataRequest request);
        Task<AggregationResult> AggregateMetricsAsync(AggregationQuery query);
        Task<CorrelationAnalysis> FindCorrelationsAsync(CorrelationParameters parameters);
        Task<SegmentationResult> SegmentDataAsync(SegmentationCriteria criteria);
        
        // Export & Reporting
        Task<ExportResult> ExportAnalyticsDataAsync(ExportRequest request);
        Task<ScheduledReport> ScheduleReportAsync(ReportSchedule schedule);
        Task<ReportStatus> GetReportStatusAsync(string reportId);
        Task<bool> CancelScheduledReportAsync(string reportId);
    }

    // Supporting Models
    public class AnalyticsQuery
    {
        public string QueryType { get; set; }
        public Dictionary<string, object> Parameters { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<string> Dimensions { get; set; }
        public List<string> Metrics { get; set; }
        public string GroupBy { get; set; }
        public string OrderBy { get; set; }
        public int? Limit { get; set; }
    }

    public class AnalyticsReport
    {
        public string ReportId { get; set; }
        public DateTime GeneratedAt { get; set; }
        public TimeSpan ProcessingTime { get; set; }
        public Dictionary<string, object> Data { get; set; }
        public List<ChartData> Charts { get; set; }
        public List<TableData> Tables { get; set; }
        public Dictionary<string, double> Summary { get; set; }
        public List<Insight> Insights { get; set; }
    }

    public class BusinessEvent
    {
        public string EventId { get; set; }
        public string EventType { get; set; }
        public DateTime Timestamp { get; set; }
        public string Source { get; set; }
        public Dictionary<string, object> Payload { get; set; }
        public string UserId { get; set; }
        public string SessionId { get; set; }
        public Dictionary<string, string> Context { get; set; }
    }

    public class StreamAnalyticsResult
    {
        public string StreamId { get; set; }
        public bool Processed { get; set; }
        public DateTime ProcessedAt { get; set; }
        public Dictionary<string, object> Results { get; set; }
        public List<string> TriggeredAlerts { get; set; }
        public TimeSpan ProcessingLatency { get; set; }
    }

    public class ForecastResult
    {
        public string ForecastId { get; set; }
        public string ModelType { get; set; }
        public DateTime GeneratedAt { get; set; }
        public List<ForecastPoint> Predictions { get; set; }
        public double ConfidenceLevel { get; set; }
        public double MeanAbsoluteError { get; set; }
        public Dictionary<string, double> ModelMetrics { get; set; }
        public List<string> Recommendations { get; set; }
    }

    public class ForecastPoint
    {
        public DateTime Date { get; set; }
        public double Value { get; set; }
        public double LowerBound { get; set; }
        public double UpperBound { get; set; }
        public double Confidence { get; set; }
    }

    public class BusinessInsights
    {
        public List<KeyPerformanceIndicator> KPIs { get; set; }
        public List<Trend> Trends { get; set; }
        public List<Opportunity> Opportunities { get; set; }
        public List<Risk> Risks { get; set; }
        public Dictionary<string, object> ExecutiveSummary { get; set; }
    }

    public class ChartData
    {
        public string ChartType { get; set; }
        public string Title { get; set; }
        public List<Series> Series { get; set; }
        public AxisConfiguration XAxis { get; set; }
        public AxisConfiguration YAxis { get; set; }
    }

    public class TableData
    {
        public string Title { get; set; }
        public List<string> Columns { get; set; }
        public List<Dictionary<string, object>> Rows { get; set; }
        public Dictionary<string, object> Totals { get; set; }
    }
}