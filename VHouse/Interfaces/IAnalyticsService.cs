using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VHouse.Classes;

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
        Task<SegmentationResult> SegmentDataAsync(AnalyticsSegmentationCriteria criteria);
        
        // Export & Reporting
        Task<ExportResult> ExportAnalyticsDataAsync(ExportRequest request);
        Task<ScheduledReport> ScheduleReportAsync(ReportSchedule schedule);
        Task<ReportStatus> GetReportStatusAsync(string reportId);
        Task<bool> CancelScheduledReportAsync(string reportId);
    }
}