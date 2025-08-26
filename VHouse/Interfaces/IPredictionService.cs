using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace VHouse.Interfaces
{
    public interface IPredictionService
    {
        // Demand Forecasting
        Task<DemandForecast> PredictProductDemandAsync(string productId, int daysAhead);
        Task<InventoryForecast> PredictInventoryNeedsAsync(DateTime targetDate);
        Task<SeasonalForecast> PredictSeasonalTrendsAsync(string category, int monthsAhead);
        
        // Sales Predictions
        Task<SalesPrediction> PredictSalesAsync(DateTime startDate, DateTime endDate);
        Task<RevenueProjection> ProjectRevenueAsync(ProjectionParameters parameters);
        Task<CustomerLifetimeValue> PredictCustomerValueAsync(string customerId);
        
        // Customer Analytics
        Task<ChurnPrediction> PredictCustomerChurnAsync(string customerId);
        Task<NextPurchasePrediction> PredictNextPurchaseAsync(string customerId);
        Task<ProductRecommendations> GetPersonalizedRecommendationsAsync(string customerId, int count);
        
        // Price Optimization
        Task<OptimalPrice> CalculateOptimalPriceAsync(string productId);
        Task<PriceElasticity> AnalyzePriceElasticityAsync(string productId);
        Task<PromotionEffectiveness> PredictPromotionImpactAsync(PromotionScenario scenario);
        
        // Model Management
        Task<ModelStatus> TrainModelAsync(string modelType, TrainingData data);
        Task<ModelPerformance> EvaluateModelAsync(string modelId);
        Task<bool> UpdateModelAsync(string modelId, UpdateParameters parameters);
        Task<List<PredictionModel>> GetAvailableModelsAsync();
    }

    public class DemandForecast
    {
        public string ProductId { get; set; }
        public List<DailyDemand> Predictions { get; set; }
        public double ConfidenceLevel { get; set; }
        public Dictionary<string, object> Factors { get; set; }
    }

    public class DailyDemand
    {
        public DateTime Date { get; set; }
        public double ExpectedDemand { get; set; }
        public double MinDemand { get; set; }
        public double MaxDemand { get; set; }
    }

    public class SalesPrediction
    {
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public double PredictedSales { get; set; }
        public double ConfidenceInterval { get; set; }
        public List<SalesFactor> ContributingFactors { get; set; }
    }

    public class ChurnPrediction
    {
        public string CustomerId { get; set; }
        public double ChurnProbability { get; set; }
        public List<string> RiskFactors { get; set; }
        public List<string> RetentionStrategies { get; set; }
    }

    public class ProductRecommendations
    {
        public string CustomerId { get; set; }
        public List<RecommendedProduct> Products { get; set; }
        public string RecommendationType { get; set; }
    }

    public class RecommendedProduct
    {
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public double RecommendationScore { get; set; }
        public string Reason { get; set; }
    }

    public class PredictionModel
    {
        public string ModelId { get; set; }
        public string ModelType { get; set; }
        public DateTime TrainedDate { get; set; }
        public double Accuracy { get; set; }
        public string Status { get; set; }
    }

    // Additional supporting classes
    public class InventoryForecast { }
    public class SeasonalForecast { }
    public class RevenueProjection { }
    public class ProjectionParameters { }
    public class CustomerLifetimeValue { }
    public class NextPurchasePrediction { }
    public class OptimalPrice { }
    public class PriceElasticity { }
    public class PromotionEffectiveness { }
    public class PromotionScenario { }
    public class ModelStatus { }
    public class TrainingData { }
    public class ModelPerformance { }
    public class UpdateParameters { }
    public class SalesFactor { }
}