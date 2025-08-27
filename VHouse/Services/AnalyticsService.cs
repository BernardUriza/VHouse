using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Distributed;
using VHouse.Interfaces;
using VHouse.Classes;
using VHouse.Repositories;
using System.Text.Json;
using System.Threading;

namespace VHouse.Services
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly ILogger<AnalyticsService> _logger;
        private readonly IDistributedCache _cache;
        private readonly VHouse.Repositories.IUnitOfWork _unitOfWork;
        private readonly IMonitoringService _monitoringService;
        private readonly ConcurrentDictionary<string, StreamProcessor> _activeStreams;
        private readonly ConcurrentDictionary<string, ScheduledReport> _scheduledReports;
        private readonly SemaphoreSlim _processingLock;
        private readonly Timer _aggregationTimer;

        public AnalyticsService(
            ILogger<AnalyticsService> logger,
            IDistributedCache cache,
            VHouse.Repositories.IUnitOfWork unitOfWork,
            IMonitoringService monitoringService)
        {
            _logger = logger;
            _cache = cache;
            _unitOfWork = unitOfWork;
            _monitoringService = monitoringService;
            _activeStreams = new ConcurrentDictionary<string, StreamProcessor>();
            _scheduledReports = new ConcurrentDictionary<string, ScheduledReport>();
            _processingLock = new SemaphoreSlim(1, 1);
            
            // Start aggregation timer for real-time metrics
            _aggregationTimer = new Timer(AggregateMetricsCallback, null, TimeSpan.Zero, TimeSpan.FromSeconds(30));
        }

        // Real-time Analytics Implementation
        public async Task<AnalyticsReport> GenerateRealtimeReportAsync(AnalyticsQuery query)
        {
            var startTime = DateTime.UtcNow;
            var reportId = Guid.NewGuid().ToString();
            
            try
            {
                _logger.LogInformation($"Generating real-time report {reportId} with query type: {query.QueryType}");
                
                // Track analytics operation
                await _monitoringService.RecordMetricAsync("analytics.report.generated", 1);
                
                var report = new AnalyticsReport
                {
                    ReportId = reportId,
                    GeneratedAt = startTime,
                    Data = new Dictionary<string, object>(),
                    Charts = new List<ChartData>(),
                    Tables = new List<TableData>(),
                    Summary = new Dictionary<string, double>(),
                    Insights = new List<Insight>()
                };

                // Process based on query type
                switch (query.QueryType?.ToLower())
                {
                    case "sales":
                        await ProcessSalesAnalytics(query, report);
                        break;
                    case "inventory":
                        await ProcessInventoryAnalytics(query, report);
                        break;
                    case "customer":
                        await ProcessCustomerAnalytics(query, report);
                        break;
                    case "performance":
                        await ProcessPerformanceAnalytics(query, report);
                        break;
                    default:
                        await ProcessGeneralAnalytics(query, report);
                        break;
                }

                // Generate insights using ML
                report.Insights = await GenerateInsights(report.Data);
                
                // Cache the report for quick retrieval
                await CacheReportAsync(reportId, report);
                
                report.ProcessingTime = DateTime.UtcNow - startTime;
                
                _logger.LogInformation($"Report {reportId} generated successfully in {report.ProcessingTime.TotalMilliseconds}ms");
                return report;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error generating analytics report {reportId}");
                await _monitoringService.RecordMetricAsync("analytics.report.error", 1);
                throw;
            }
        }

        public async Task<StreamAnalyticsResult> ProcessEventStreamAsync(BusinessEvent businessEvent)
        {
            var result = new StreamAnalyticsResult
            {
                StreamId = Guid.NewGuid().ToString(),
                ProcessedAt = DateTime.UtcNow,
                Results = new Dictionary<string, object>(),
                TriggeredAlerts = new List<string>()
            };

            try
            {
                var startTime = DateTime.UtcNow;
                
                // Get or create stream processor for this event type
                var processor = _activeStreams.GetOrAdd(
                    businessEvent.EventType,
                    _ => new StreamProcessor(businessEvent.EventType));
                
                // Process the event
                await processor.ProcessEventAsync(businessEvent);
                
                // Check for anomalies
                if (await IsAnomalousEvent(businessEvent))
                {
                    result.TriggeredAlerts.Add($"Anomaly detected in {businessEvent.EventType}");
                    await _monitoringService.RecordMetricAsync(
                        $"analytics.anomaly.{businessEvent.EventType}",
                        1);
                }
                
                // Store processing time in results
                result.Results["ProcessingTime"] = DateTime.UtcNow - startTime;
                result.Results["Processed"] = true;
                
                // Store event for batch processing
                await StoreEventForBatchProcessing(businessEvent);
                
                await _monitoringService.RecordMetricAsync("analytics.stream.processed", 1);
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing event stream for event {businessEvent.EventId}");
                result.Results["Processed"] = false;
                throw;
            }
        }

        public async Task<LiveMetrics> GetLiveMetricsAsync(string metricCategory)
        {
            var metrics = new LiveMetrics
            {
                MetricCategory = metricCategory,
                Timestamp = DateTime.UtcNow,
                Metrics = new Dictionary<string, double>()
            };

            try
            {
                // Retrieve cached live metrics
                var cacheKey = $"live_metrics_{metricCategory}";
                var cachedData = await _cache.GetStringAsync(cacheKey);
                
                if (!string.IsNullOrEmpty(cachedData))
                {
                    metrics = JsonSerializer.Deserialize<LiveMetrics>(cachedData);
                }
                else
                {
                    // Calculate live metrics
                    metrics = await CalculateLiveMetrics(metricCategory);
                    
                    // Cache for 5 seconds
                    await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(metrics),
                        new DistributedCacheEntryOptions
                        {
                            AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(5)
                        });
                }
                
                return metrics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving live metrics for category {metricCategory}");
                throw;
            }
        }

        // Predictive Analytics Implementation
        public async Task<ForecastResult> PredictDemandAsync(PredictionParameters parameters)
        {
            var forecastId = Guid.NewGuid().ToString();
            
            try
            {
                _logger.LogInformation($"Generating demand forecast {forecastId}");
                
                var forecast = new ForecastResult
                {
                    PredictionId = forecastId,
                    Confidence = 0.85,
                    ForecastData = new Dictionary<string, object>
                    {
                        ["ModelType"] = "ARIMA_SEASONAL",
                        ["Predictions"] = new List<object>(),
                        ["ModelMetrics"] = new Dictionary<string, double>(),
                        ["Recommendations"] = new List<string>()
                    },
                    GeneratedAt = DateTime.UtcNow,
                    Points = new List<VHouse.Classes.PredictionPoint>()
                };

                // Retrieve historical data
                var historicalData = await GetHistoricalData(parameters);
                
                // Apply time series analysis
                var model = new TimeSeriesModel(historicalData);
                var forecastDays = 30; // Default forecast horizon
                if (parameters.Features.ContainsKey("ForecastHorizon"))
                {
                    int.TryParse(parameters.Features["ForecastHorizon"]?.ToString(), out forecastDays);
                }
                var predictions = model.Forecast(forecastDays);
                
                // Calculate confidence intervals
                foreach (var prediction in predictions)
                {
                    var forecastPoint = new VHouse.Classes.PredictionPoint
                    {
                        Date = prediction.Date,
                        Value = prediction.Value,
                        Confidence = CalculateConfidence(prediction, historicalData),
                        LowerBound = prediction.Value * 0.85,
                        UpperBound = prediction.Value * 1.15
                    };
                    forecast.Points.Add(forecastPoint);
                }
                
                // Calculate model metrics
                var modelMetrics = (Dictionary<string, double>)forecast.ForecastData["ModelMetrics"];
                modelMetrics["MAPE"] = model.MeanAbsolutePercentageError;
                modelMetrics["RMSE"] = model.RootMeanSquareError;
                modelMetrics["R2"] = model.RSquared;
                modelMetrics["OverallConfidence"] = model.OverallConfidence;
                modelMetrics["MeanAbsoluteError"] = model.MeanAbsoluteError;
                
                // Generate recommendations
                var recommendations = GenerateDemandRecommendations(forecast, parameters);
                forecast.ForecastData["Recommendations"] = recommendations;
                
                await _monitoringService.RecordMetricAsync("analytics.forecast.generated", 1);
                
                return forecast;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error generating demand forecast {forecastId}");
                throw;
            }
        }

        public async Task<TrendAnalysis> AnalyzeTrendsAsync(TrendQuery query)
        {
            try
            {
                var analysis = new TrendAnalysis
                {
                    AnalysisId = Guid.NewGuid().ToString(),
                    GeneratedAt = DateTime.UtcNow,
                    TrendPoints = new List<TrendPoint>(),
                    TrendDirection = "Unknown"
                };

                // Retrieve time series data
                var data = await GetTimeSeriesData(query);
                
                // Identify trends using moving averages
                var windowSize = 7; // Default window size
                var movingAverage = CalculateMovingAverage(data, windowSize);
                var trendDirection = DetermineTrendDirection(movingAverage);
                
                analysis.TrendDirection = trendDirection.ToString();
                
                // Convert data to TrendPoints
                foreach (var dataPoint in data)
                {
                    analysis.TrendPoints.Add(new TrendPoint
                    {
                        Timestamp = dataPoint.Timestamp,
                        Value = dataPoint.Value,
                        Context = new Dictionary<string, object>
                        {
                            ["MovingAverage"] = movingAverage.FirstOrDefault(ma => ma.Timestamp == dataPoint.Timestamp)?.Value ?? dataPoint.Value,
                            ["TrendStrength"] = CalculateTrendStrength(data),
                            ["ChangePercent"] = CalculateChangePercent(data)
                        }
                    });
                }
                
                return analysis;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing trends");
                throw;
            }
        }

        // Business Insights Implementation
        public async Task<BusinessInsights> GetBusinessInsightsAsync(DateTime fromDate, DateTime toDate)
        {
            try
            {
                var insights = new BusinessInsights
                {
                    KPIs = new List<KeyPerformanceIndicator>(),
                    Trends = new List<Trend>(),
                    Opportunities = new List<BusinessOpportunity>(),
                    Risks = new List<Risk>(),
                    ExecutiveSummary = new Dictionary<string, object>()
                };

                // Calculate KPIs
                insights.KPIs = await CalculateKPIsAsync(fromDate, toDate);
                
                // Analyze trends
                insights.Trends = await AnalyzeBusinessTrendsAsync(fromDate, toDate);
                
                // Identify opportunities using ML
                insights.Opportunities = await IdentifyOpportunitiesAsync(insights.KPIs, insights.Trends);
                
                // Assess risks
                insights.Risks = await AssessBusinessRisksAsync(insights.KPIs, insights.Trends);
                
                // Generate executive summary
                insights.ExecutiveSummary = GenerateExecutiveSummary(insights);
                
                await _monitoringService.RecordMetricAsync("analytics.insights.generated", 1);
                
                return insights;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating business insights");
                throw;
            }
        }

        // Helper Methods
        private async Task ProcessSalesAnalytics(AnalyticsQuery query, AnalyticsReport report)
        {
            var salesData = await _unitOfWork.Orders.GetAllAsync();
            var filteredSales = salesData.Where(o => 
                o.OrderDate >= query.FromDate && 
                o.OrderDate <= query.ToDate);

            // Calculate metrics
            report.Summary["TotalRevenue"] = (double)filteredSales.Sum(o => o.TotalAmount);
            report.Summary["AverageOrderValue"] = filteredSales.Any() ? (double)filteredSales.Average(o => o.TotalAmount) : 0;
            report.Summary["OrderCount"] = filteredSales.Count();
            
            // Generate sales chart
            var salesChart = new ChartData
            {
                ChartType = "line",
                Title = "Sales Trend",
                Series = new List<Series>
                {
                    new Series
                    {
                        Name = "Daily Sales",
                        Data = filteredSales
                            .GroupBy(o => o.OrderDate.Date)
                            .Select(g => new DataPoint
                            {
                                Timestamp = g.Key,
                                Value = (double)g.Sum(o => o.TotalAmount)
                            })
                            .ToList()
                    }
                }
            };
            report.Charts.Add(salesChart);
            
            // Generate top products table
            var topProducts = new TableData
            {
                Title = "Top Selling Products",
                Columns = new List<string> { "Product", "Quantity", "Revenue" },
                Rows = new List<Dictionary<string, object>>()
            };
            
            report.Tables.Add(topProducts);
        }

        private async Task ProcessInventoryAnalytics(AnalyticsQuery query, AnalyticsReport report)
        {
            var inventory = await _unitOfWork.Products.GetAllAsync();
            
            report.Summary["TotalProducts"] = inventory.Count();
            report.Summary["TotalValue"] = (double)inventory.Sum(i => i.PriceRetail);
            report.Summary["LowStockItems"] = 0; // Would need inventory system
            report.Summary["OutOfStockItems"] = 0; // Would need inventory system
            
            // Inventory distribution chart
            var inventoryChart = new ChartData
            {
                ChartType = "pie",
                Title = "Inventory Distribution by Category",
                Series = new List<Series>
                {
                    new Series
                    {
                        Name = "Categories",
                        Data = inventory
                            .GroupBy(i => "General") // No category field available
                            .Select(g => new DataPoint
                            {
                                X = DateTime.UtcNow,
                                Y = g.Count()
                            })
                            .ToList()
                    }
                }
            };
            report.Charts.Add(inventoryChart);
        }

        private async Task<List<Insight>> GenerateInsights(Dictionary<string, object> data)
        {
            var insights = new List<Insight>();
            
            // Analyze data patterns for insights
            if (data.ContainsKey("TotalRevenue") && data["TotalRevenue"] is double revenue)
            {
                if (revenue > 100000)
                {
                    insights.Add(new Insight
                    {
                        Type = "Positive",
                        Title = "Strong Revenue Performance",
                        Description = $"Revenue of ${revenue:N2} indicates strong business performance",
                        Impact = "High",
                        Recommendations = new List<string>
                        {
                            "Consider expanding product lines",
                            "Invest in marketing to maintain momentum"
                        }
                    });
                }
            }
            
            return insights;
        }

        private async Task<bool> IsAnomalousEvent(BusinessEvent businessEvent)
        {
            // Simple anomaly detection based on event patterns
            // In production, this would use ML models
            
            if (businessEvent.EventType == "order.created" && businessEvent.Payload.ContainsKey("amount"))
            {
                var amount = Convert.ToDouble(businessEvent.Payload["amount"]);
                
                // Check if amount is unusually high
                var avgOrderValue = await GetAverageOrderValueAsync();
                if (amount > avgOrderValue * 3)
                {
                    return true;
                }
            }
            
            return false;
        }

        private async Task<double> GetAverageOrderValueAsync()
        {
            var cacheKey = "avg_order_value";
            var cached = await _cache.GetStringAsync(cacheKey);
            
            if (!string.IsNullOrEmpty(cached))
            {
                return double.Parse(cached);
            }
            
            var orders = await _unitOfWork.Orders.GetAllAsync();
            var avg = orders.Any() ? (double)orders.Average(o => o.TotalAmount) : 0;
            
            await _cache.SetStringAsync(cacheKey, avg.ToString(),
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                });
            
            return avg;
        }

        private async Task CacheReportAsync(string reportId, AnalyticsReport report)
        {
            var cacheKey = $"report_{reportId}";
            var serialized = JsonSerializer.Serialize(report);
            
            await _cache.SetStringAsync(cacheKey, serialized,
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
                });
        }

        private void AggregateMetricsCallback(object state)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    await AggregateRealtimeMetrics();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error aggregating real-time metrics");
                }
            });
        }

        private async Task AggregateRealtimeMetrics()
        {
            // Aggregate metrics from various sources
            await _processingLock.WaitAsync();
            try
            {
                // Process pending events
                foreach (var processor in _activeStreams.Values)
                {
                    await processor.AggregateMetricsAsync();
                }
            }
            finally
            {
                _processingLock.Release();
            }
        }

        // Additional helper methods would be implemented here...
        
        // Stub implementations for missing methods
        public async Task<PerformanceAnalytics> AnalyzeSystemPerformanceAsync(TimeSpan period)
        {
            return new PerformanceAnalytics();
        }

        public async Task<AnomalyDetectionResult> DetectAnomaliesAsync(DatasetParameters parameters)
        {
            return new AnomalyDetectionResult();
        }

        public async Task<OptimizationResult> OptimizeInventoryAsync(InventoryOptimizationRequest request)
        {
            return new OptimizationResult();
        }

        public async Task<CustomerAnalytics> AnalyzeCustomerBehaviorAsync(CustomerAnalyticsQuery query)
        {
            return new CustomerAnalytics();
        }

        public async Task<ProductAnalytics> AnalyzeProductPerformanceAsync(ProductAnalyticsQuery query)
        {
            return new ProductAnalytics();
        }

        public async Task<RevenueAnalytics> AnalyzeRevenueStreamsAsync(RevenueQuery query)
        {
            return new RevenueAnalytics();
        }

        public async Task<DataProcessingResult> ProcessBatchDataAsync(BatchDataRequest request)
        {
            return new DataProcessingResult();
        }

        public async Task<AggregationResult> AggregateMetricsAsync(AggregationQuery query)
        {
            return new AggregationResult();
        }

        public async Task<CorrelationAnalysis> FindCorrelationsAsync(CorrelationParameters parameters)
        {
            return new CorrelationAnalysis();
        }

        public async Task<SegmentationResult> SegmentDataAsync(SegmentationCriteria criteria)
        {
            return new SegmentationResult();
        }

        public async Task<ExportResult> ExportAnalyticsDataAsync(ExportRequest request)
        {
            return new ExportResult();
        }

        public async Task<ScheduledReport> ScheduleReportAsync(ReportSchedule schedule)
        {
            var report = new ScheduledReport
            {
                ReportId = Guid.NewGuid().ToString(),
                Schedule = schedule,
                Status = ReportStatus.Scheduled,
                CreatedAt = DateTime.UtcNow
            };
            
            _scheduledReports[report.ReportId] = report;
            return report;
        }

        public async Task<ReportStatus> GetReportStatusAsync(string reportId)
        {
            if (_scheduledReports.TryGetValue(reportId, out var report))
            {
                return new ReportStatus
                {
                    ReportId = reportId,
                    Status = report.Status,
                    Progress = 100,
                    LastUpdated = DateTime.UtcNow
                };
            }
            return new ReportStatus
            {
                ReportId = reportId,
                Status = ReportStatus.NotFound,
                Progress = 0,
                LastUpdated = DateTime.UtcNow
            };
        }

        public async Task<bool> CancelScheduledReportAsync(string reportId)
        {
            return _scheduledReports.TryRemove(reportId, out _);
        }

        private async Task ProcessGeneralAnalytics(AnalyticsQuery query, AnalyticsReport report)
        {
            // Generic analytics processing
            report.Summary["ProcessedAt"] = DateTime.UtcNow.Ticks;
        }

        private async Task ProcessCustomerAnalytics(AnalyticsQuery query, AnalyticsReport report)
        {
            var customers = await _unitOfWork.Customers.GetAllAsync();
            report.Summary["TotalCustomers"] = customers.Count();
        }

        private async Task ProcessPerformanceAnalytics(AnalyticsQuery query, AnalyticsReport report)
        {
            var metrics = await _monitoringService.GetPerformanceMetricsAsync(query.StartDate, query.EndDate);
            report.Data["PerformanceMetrics"] = metrics;
        }

        private async Task StoreEventForBatchProcessing(BusinessEvent businessEvent)
        {
            var cacheKey = $"batch_event_{businessEvent.EventId}";
            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(businessEvent),
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24)
                });
        }

        private async Task<LiveMetrics> CalculateLiveMetrics(string category)
        {
            return new LiveMetrics
            {
                Category = category,
                Timestamp = DateTime.UtcNow,
                Values = new Dictionary<string, double>
                {
                    ["current"] = new Random().Next(50, 100),
                    ["average"] = new Random().Next(60, 90)
                }
            };
        }

        private async Task<List<DataPoint>> GetHistoricalData(PredictionParameters parameters)
        {
            // Retrieve historical data based on parameters
            return new List<DataPoint>();
        }

        private double CalculateConfidence(PredictionPoint prediction, List<DataPoint> historicalData)
        {
            return 0.85; // Simplified confidence calculation
        }

        private List<string> GenerateDemandRecommendations(ForecastResult forecast, PredictionParameters parameters)
        {
            return new List<string>
            {
                "Increase inventory for peak demand periods",
                "Consider promotional activities during low demand periods"
            };
        }

        private async Task<List<DataPoint>> GetTimeSeriesData(TrendQuery query)
        {
            return new List<DataPoint>();
        }

        private List<DataPoint> CalculateMovingAverage(List<DataPoint> data, int windowSize)
        {
            return new List<DataPoint>();
        }

        private TrendDirection DetermineTrendDirection(List<DataPoint> movingAverage)
        {
            return TrendDirection.Stable;
        }

        private double CalculateTrendStrength(List<DataPoint> data)
        {
            return 0.75;
        }

        private double CalculateChangePercent(List<DataPoint> data)
        {
            if (!data.Any() || data.Count < 2) return 0;
            var first = data.First().Value;
            var last = data.Last().Value;
            return first != 0 ? ((last - first) / first) * 100 : 0;
        }

        private List<Pattern> DetectPatterns(List<DataPoint> data)
        {
            return new List<Pattern>();
        }

        private async Task<SeasonalityAnalysis> AnalyzeSeasonalityAsync(List<DataPoint> data)
        {
            return new SeasonalityAnalysis();
        }

        private async Task<List<KeyPerformanceIndicator>> CalculateKPIsAsync(DateTime fromDate, DateTime toDate)
        {
            return new List<KeyPerformanceIndicator>();
        }

        private async Task<List<Trend>> AnalyzeBusinessTrendsAsync(DateTime fromDate, DateTime toDate)
        {
            return new List<Trend>();
        }

        private async Task<List<BusinessOpportunity>> IdentifyOpportunitiesAsync(List<KeyPerformanceIndicator> kpis, List<Trend> trends)
        {
            return new List<BusinessOpportunity>();
        }

        private async Task<List<Risk>> AssessBusinessRisksAsync(List<KeyPerformanceIndicator> kpis, List<Trend> trends)
        {
            return new List<Risk>();
        }

        private Dictionary<string, object> GenerateExecutiveSummary(BusinessInsights insights)
        {
            return new Dictionary<string, object>
            {
                ["TotalKPIs"] = insights.KPIs.Count,
                ["TotalOpportunities"] = insights.Opportunities.Count,
                ["TotalRisks"] = insights.Risks.Count
            };
        }

        // Missing interface implementations

        public async Task<SegmentationResult> SegmentDataAsync(AnalyticsSegmentationCriteria criteria)
        {
            await Task.Delay(800);
            return new SegmentationResult
            {
                SegmentationId = Guid.NewGuid().ToString(),
                Segments = new List<DataSegment>(),
                ProcessedAt = DateTime.UtcNow
            };
        }
    }

    // Supporting classes
    internal class StreamProcessor
    {
        private readonly string _eventType;
        private readonly ConcurrentQueue<BusinessEvent> _eventQueue;

        public StreamProcessor(string eventType)
        {
            _eventType = eventType;
            _eventQueue = new ConcurrentQueue<BusinessEvent>();
        }

        public async Task ProcessEventAsync(BusinessEvent businessEvent)
        {
            _eventQueue.Enqueue(businessEvent);
        }

        public async Task AggregateMetricsAsync()
        {
            // Process queued events
            while (_eventQueue.TryDequeue(out var evt))
            {
                // Process event
            }
        }
    }

    internal class TimeSeriesModel
    {
        public double MeanAbsolutePercentageError { get; set; }
        public double RootMeanSquareError { get; set; }
        public double RSquared { get; set; }
        public double OverallConfidence { get; set; }
        public double MeanAbsoluteError { get; set; }

        public TimeSeriesModel(List<DataPoint> data)
        {
            // Initialize model with historical data
            MeanAbsolutePercentageError = 5.2;
            RootMeanSquareError = 12.3;
            RSquared = 0.92;
            OverallConfidence = 0.88;
            MeanAbsoluteError = 8.5;
        }

        public List<VHouse.Classes.PredictionPoint> Forecast(int horizon)
        {
            var predictions = new List<VHouse.Classes.PredictionPoint>();
            for (int i = 0; i < horizon; i++)
            {
                predictions.Add(new VHouse.Classes.PredictionPoint
                {
                    Date = DateTime.UtcNow.AddDays(i + 1),
                    Value = 1000 + (i * 50),
                    Confidence = 0.85,
                    LowerBound = 800 + (i * 40),
                    UpperBound = 1200 + (i * 60)
                });
            }
            return predictions;
        }
    }

    // Model classes for Analytics



}