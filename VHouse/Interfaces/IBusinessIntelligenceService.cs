using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VHouse.Classes;

namespace VHouse.Interfaces
{
    public interface IBusinessIntelligenceService
    {
        // Dashboard Generation
        Task<Dashboard> GenerateExecutiveDashboardAsync();
        Task<Dashboard> GenerateOperationalDashboardAsync();
        Task<Dashboard> GenerateSalesDashboardAsync();
        Task<Dashboard> GenerateFinancialDashboardAsync();
        
        // KPI Management
        Task<KpiMetrics> CalculateKpiMetricsAsync(KpiRequest request);
        Task<List<KeyPerformanceIndicator>> GetRealTimeKpisAsync();
        Task<KpiComparison> CompareKpisAsync(ComparisonRequest request);
        Task<KpiTarget> SetKpiTargetAsync(string kpiName, double targetValue);
        Task<List<KpiAlert>> GetKpiAlertsAsync();
        
        // Advanced Analytics
        Task<TrendAnalysis> AnalyzeTrendsAsync(TrendQuery query);
        Task<CohortAnalysis> PerformCohortAnalysisAsync(CohortRequest request);
        Task<FunnelAnalysis> AnalyzeFunnelAsync(FunnelRequest request);
        Task<RetentionAnalysis> AnalyzeRetentionAsync(RetentionRequest request);
        
        // Reporting
        Task ScheduleAutomatedReportsAsync(ReportSchedule schedule);
        Task<Report> GenerateCustomReportAsync(CustomReportRequest request);
        Task<ReportTemplate> CreateReportTemplateAsync(ReportTemplateDefinition definition);
        Task<List<Report>> GetScheduledReportsAsync();
        
        // Data Mining
        Task<DataMiningResult> DiscoverPatternsAsync(DataMiningRequest request);
        Task<AssociationRules> FindAssociationRulesAsync(AssociationRequest request);
        Task<ClusteringResult> PerformClusterAnalysisAsync(ClusteringRequest request);
        
        // Benchmarking
        Task<BenchmarkResult> BenchmarkPerformanceAsync(BenchmarkCriteria criteria);
        Task<CompetitiveAnalysis> AnalyzeCompetitorsAsync(CompetitorData data);
        Task<MarketAnalysis> AnalyzeMarketTrendsAsync(MarketRequest request);
    }

    // Dashboard Models
    public class Dashboard
    {
        public string DashboardId { get; set; }
        public string Type { get; set; }
        public DateTime GeneratedAt { get; set; }
        public List<Widget> Widgets { get; set; }
        public Dictionary<string, object> Filters { get; set; }
        public RefreshSettings RefreshSettings { get; set; }
    }

    public class Widget
    {
        public string WidgetId { get; set; }
        public string Type { get; set; } // chart, table, metric, gauge
        public string Title { get; set; }
        public WidgetData Data { get; set; }
        public WidgetConfiguration Configuration { get; set; }
        public Position Position { get; set; }
        public Size Size { get; set; }
    }

    public class WidgetData
    {
        public object Value { get; set; }
        public List<Series> Series { get; set; }
        public Dictionary<string, object> Metadata { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    public class WidgetConfiguration
    {
        public string ColorScheme { get; set; }
        public bool ShowLegend { get; set; }
        public bool Interactive { get; set; }
        public Dictionary<string, object> ChartOptions { get; set; }
    }

    // KPI Models
    public class KpiRequest
    {
        public List<string> KpiNames { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Granularity { get; set; } // daily, weekly, monthly
        public Dictionary<string, object> Filters { get; set; }
    }

    public class KpiMetrics
    {
        public List<KeyPerformanceIndicator> KPIs { get; set; }
        public Dictionary<string, TrendInfo> Trends { get; set; }
        public Dictionary<string, double> Comparisons { get; set; }
        public List<string> Warnings { get; set; }
    }

    public class KpiComparison
    {
        public string ComparisonId { get; set; }
        public Period CurrentPeriod { get; set; }
        public Period ComparisonPeriod { get; set; }
        public Dictionary<string, ComparisonMetric> Metrics { get; set; }
    }

    public class ComparisonMetric
    {
        public double CurrentValue { get; set; }
        public double ComparisonValue { get; set; }
        public double AbsoluteChange { get; set; }
        public double PercentageChange { get; set; }
        public string Trend { get; set; }
    }

    // Cohort Analysis Models
    public class CohortRequest
    {
        public string CohortType { get; set; } // user, product, revenue
        public string GroupBy { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string MetricToTrack { get; set; }
    }

    public class CohortAnalysis
    {
        public string AnalysisId { get; set; }
        public List<Cohort> Cohorts { get; set; }
        public CohortMatrix Matrix { get; set; }
        public List<CohortInsight> Insights { get; set; }
    }

    public class Cohort
    {
        public string CohortId { get; set; }
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public int Size { get; set; }
        public Dictionary<int, double> RetentionByPeriod { get; set; }
    }

    // Funnel Analysis Models
    public class FunnelRequest
    {
        public string FunnelName { get; set; }
        public List<FunnelStep> Steps { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class FunnelAnalysis
    {
        public string AnalysisId { get; set; }
        public List<FunnelStepResult> Steps { get; set; }
        public double OverallConversionRate { get; set; }
        public List<DropoffPoint> DropoffPoints { get; set; }
        public List<string> Recommendations { get; set; }
    }

    public class FunnelStep
    {
        public string StepName { get; set; }
        public string EventName { get; set; }
        public Dictionary<string, object> Filters { get; set; }
    }

    public class FunnelStepResult
    {
        public string StepName { get; set; }
        public int UsersEntered { get; set; }
        public int UsersCompleted { get; set; }
        public double ConversionRate { get; set; }
        public double DropoffRate { get; set; }
        public TimeSpan AverageTimeToComplete { get; set; }
    }

    // Data Mining Models
    public class DataMiningRequest
    {
        public string DatasetId { get; set; }
        public List<string> Algorithms { get; set; }
        public Dictionary<string, object> Parameters { get; set; }
    }

    public class DataMiningResult
    {
        public string ResultId { get; set; }
        public List<Pattern> DiscoveredPatterns { get; set; }
        public List<Rule> Rules { get; set; }
        public Dictionary<string, object> Statistics { get; set; }
    }

    public class AssociationRules
    {
        public List<Rule> Rules { get; set; }
        public double MinSupport { get; set; }
        public double MinConfidence { get; set; }
    }

    public class Rule
    {
        public List<string> Antecedent { get; set; }
        public List<string> Consequent { get; set; }
        public double Support { get; set; }
        public double Confidence { get; set; }
        public double Lift { get; set; }
    }

    // Benchmarking Models
    public class BenchmarkCriteria
    {
        public List<string> Metrics { get; set; }
        public string IndustryCategory { get; set; }
        public string CompanySize { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
    }

    public class BenchmarkResult
    {
        public string BenchmarkId { get; set; }
        public Dictionary<string, BenchmarkMetric> Metrics { get; set; }
        public int OverallRank { get; set; }
        public double OverallScore { get; set; }
        public List<string> StrengthAreas { get; set; }
        public List<string> ImprovementAreas { get; set; }
    }

    public class BenchmarkMetric
    {
        public double YourValue { get; set; }
        public double IndustryAverage { get; set; }
        public double TopPerformer { get; set; }
        public int Percentile { get; set; }
        public string Rating { get; set; } // Excellent, Good, Average, Below Average, Poor
    }

    // Supporting Classes
    public class Position
    {
        public int X { get; set; }
        public int Y { get; set; }
    }

    public class Size
    {
        public int Width { get; set; }
        public int Height { get; set; }
    }

    public class RefreshSettings
    {
        public bool AutoRefresh { get; set; }
        public int RefreshIntervalSeconds { get; set; }
    }

    public class TrendInfo
    {
        public double CurrentValue { get; set; }
        public double PreviousValue { get; set; }
        public string Direction { get; set; }
        public double ChangePercent { get; set; }
    }

    public class Period
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Label { get; set; }
    }

    public class ComparisonRequest
    {
        public List<string> KpiNames { get; set; }
        public Period Period1 { get; set; }
        public Period Period2 { get; set; }
    }

    public class KpiTarget
    {
        public string KpiName { get; set; }
        public double TargetValue { get; set; }
        public DateTime TargetDate { get; set; }
        public string Status { get; set; }
    }

    public class KpiAlert
    {
        public string AlertId { get; set; }
        public string KpiName { get; set; }
        public string AlertType { get; set; }
        public string Message { get; set; }
        public DateTime TriggeredAt { get; set; }
    }

    public class CustomReportRequest
    {
        public string ReportName { get; set; }
        public List<string> DataSources { get; set; }
        public List<string> Metrics { get; set; }
        public List<string> Dimensions { get; set; }
        public Dictionary<string, object> Filters { get; set; }
    }

    public class Report
    {
        public string ReportId { get; set; }
        public string Name { get; set; }
        public DateTime GeneratedAt { get; set; }
        public object Content { get; set; }
        public string Format { get; set; }
    }

    public class ReportTemplate
    {
        public string TemplateId { get; set; }
        public string Name { get; set; }
        public ReportTemplateDefinition Definition { get; set; }
    }

    public class ReportTemplateDefinition
    {
        public string Name { get; set; }
        public List<string> Sections { get; set; }
        public Dictionary<string, object> Configuration { get; set; }
    }

    public class RetentionRequest
    {
        public string CohortType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class RetentionAnalysis
    {
        public string AnalysisId { get; set; }
        public Dictionary<string, double> RetentionRates { get; set; }
        public double AverageLifetimeValue { get; set; }
        public List<string> Insights { get; set; }
    }

    public class CohortMatrix
    {
        public List<List<double>> Values { get; set; }
        public List<string> RowLabels { get; set; }
        public List<string> ColumnLabels { get; set; }
    }

    public class CohortInsight
    {
        public string Insight { get; set; }
        public string Impact { get; set; }
        public List<string> Recommendations { get; set; }
    }

    public class DropoffPoint
    {
        public string FromStep { get; set; }
        public string ToStep { get; set; }
        public double DropoffRate { get; set; }
        public int UsersLost { get; set; }
    }

    public class AssociationRequest
    {
        public string DatasetId { get; set; }
        public double MinSupport { get; set; }
        public double MinConfidence { get; set; }
    }

    public class ClusteringRequest
    {
        public string DatasetId { get; set; }
        public int NumberOfClusters { get; set; }
        public string Algorithm { get; set; }
    }

    public class ClusteringResult
    {
        public List<Cluster> Clusters { get; set; }
        public double SilhouetteScore { get; set; }
        public Dictionary<string, object> Statistics { get; set; }
    }

    public class Cluster
    {
        public string ClusterId { get; set; }
        public int Size { get; set; }
        public Dictionary<string, double> Centroid { get; set; }
        public List<string> MemberIds { get; set; }
    }

    public class CompetitorData
    {
        public List<Competitor> Competitors { get; set; }
        public List<string> MetricsToCompare { get; set; }
    }

    public class Competitor
    {
        public string Name { get; set; }
        public Dictionary<string, double> Metrics { get; set; }
    }

    public class CompetitiveAnalysis
    {
        public Dictionary<string, CompetitivePosition> Positions { get; set; }
        public List<string> CompetitiveAdvantages { get; set; }
        public List<string> CompetitiveThreats { get; set; }
    }

    public class CompetitivePosition
    {
        public int Rank { get; set; }
        public double Score { get; set; }
        public string Assessment { get; set; }
    }

    public class MarketRequest
    {
        public string MarketSegment { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class MarketAnalysis
    {
        public double MarketSize { get; set; }
        public double MarketGrowthRate { get; set; }
        public double MarketShare { get; set; }
        public List<MarketTrend> Trends { get; set; }
    }

    public class MarketTrend
    {
        public string TrendName { get; set; }
        public string Direction { get; set; }
        public double Impact { get; set; }
        public string TimeFrame { get; set; }
    }
}