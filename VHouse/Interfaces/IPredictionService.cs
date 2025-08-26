using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VHouse.Classes;

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
        Task<PredictiveProductRecommendations> GetPersonalizedRecommendationsAsync(string customerId, int count);
        
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
}