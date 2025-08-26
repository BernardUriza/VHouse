using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VHouse.Classes;

namespace VHouse.Interfaces
{
    public interface IRecommendationService
    {
        // Product Recommendations
        Task<ProductRecommendations> GetProductRecommendationsAsync(RecommendationRequest request);
        Task<ProductRecommendations> GetPersonalizedRecommendationsAsync(string userId, RecommendationContext context);
        Task<ProductRecommendations> GetSimilarProductsAsync(string productId, int count);
        Task<ProductRecommendations> GetTrendingProductsAsync(TrendingRequest request);
        Task<ProductRecommendations> GetCrossSellRecommendationsAsync(List<string> cartItems);
        
        // Customer Segmentation
        Task<CustomerSegmentation> SegmentCustomersAsync(SegmentationCriteria criteria);
        Task<CustomerProfile> GetCustomerProfileAsync(string customerId);
        Task<CustomerInsights> AnalyzeCustomerBehaviorAsync(string customerId);
        Task<List<CustomerCluster>> GetCustomerClustersAsync();
        
        // Collaborative Filtering
        Task<CollaborativeRecommendations> GetCollaborativeRecommendationsAsync(string userId);
        Task<UserSimilarity> FindSimilarUsersAsync(string userId, int count);
        Task<ItemSimilarity> FindSimilarItemsAsync(string itemId, int count);
        Task<MatrixFactorizationResult> PerformMatrixFactorizationAsync(InteractionMatrix matrix);
        
        // Content-Based Filtering
        Task<ContentBasedRecommendations> GetContentBasedRecommendationsAsync(ContentRequest request);
        Task<ItemProfile> GetItemProfileAsync(string itemId);
        Task<UserProfile> GetUserProfileAsync(string userId);
        Task<FeatureSimilarity> CalculateContentSimilarityAsync(string item1, string item2);
        
        // Real-time Recommendations
        Task<RealTimeRecommendations> GetRealTimeRecommendationsAsync(RealTimeContext context);
        Task UpdateUserInteractionAsync(UserInteraction interaction);
        Task<SessionRecommendations> GetSessionBasedRecommendationsAsync(string sessionId);
        Task<BehaviorAnalysis> AnalyzeBrowsingBehaviorAsync(BrowsingSession session);
        
        // Model Training & Management
        Task TrainRecommendationModelAsync(RecommendationModelConfig config, UserBehaviorData data);
        Task<ModelEvaluation> EvaluateModelAsync(string modelId, TestData testData);
        Task<ABTestResult> RunABTestAsync(ABTestConfig config);
        Task<ModelPerformance> GetModelPerformanceAsync(string modelId);
        
        // Business Rules & Constraints
        Task<bool> SetBusinessRulesAsync(List<BusinessRule> rules);
        Task<RecommendationFiltering> ApplyFiltersAsync(ProductRecommendations recommendations, FilterCriteria criteria);
        Task<DiversityAnalysis> AnalyzeDiversityAsync(ProductRecommendations recommendations);
        Task<ExplanableRecommendations> GetExplainableRecommendationsAsync(ExplanationRequest request);
        
        // Performance Analytics
        Task<RecommendationAnalytics> GetRecommendationAnalyticsAsync(string timeframe);
        Task<ConversionMetrics> GetConversionMetricsAsync(ConversionAnalysisRequest request);
        Task<ClickThroughAnalysis> AnalyzeClickThroughRatesAsync(string modelId);
        Task<RevenueImpact> AnalyzeRevenueImpactAsync(RevenueAnalysisRequest request);
    }
}