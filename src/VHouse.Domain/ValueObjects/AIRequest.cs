using VHouse.Domain.Enums;

namespace VHouse.Domain.ValueObjects;

public record AIRequest
{
    public string Prompt { get; init; } = string.Empty;
    public AIProvider PreferredProvider { get; init; } = AIProvider.Claude;
    public AIModel PreferredModel { get; init; } = AIModel.Claude3Sonnet;
    public double Temperature { get; init; } = 0.7;
    public int MaxTokens { get; init; } = 1000;
    public string SystemMessage { get; init; } = string.Empty;
    public Dictionary<string, object> Parameters { get; init; } = new();
}

public record ImageGenerationRequest
{
    public string Prompt { get; init; } = string.Empty;
    public AIProvider PreferredProvider { get; init; } = AIProvider.OpenAI; // DALL-E por defecto
    public AIModel PreferredModel { get; init; } = AIModel.DallE3;
    public string Style { get; init; } = "natural";
    public string Size { get; init; } = "1024x1024";
    public string Quality { get; init; } = "standard";
    public Dictionary<string, object> Parameters { get; init; } = new();
}

public record AIResponse
{
    public string Content { get; init; } = string.Empty;
    public AIProvider UsedProvider { get; init; }
    public AIModel UsedModel { get; init; }
    public bool IsSuccessful { get; init; }
    public string? ErrorMessage { get; init; }
    public int TokensUsed { get; init; }
    public TimeSpan ResponseTime { get; init; }
    public Dictionary<string, object> Metadata { get; init; } = new();
}

public record ImageGenerationResponse
{
    public byte[] ImageData { get; init; } = Array.Empty<byte>();
    public string ImageUrl { get; init; } = string.Empty;
    public AIProvider UsedProvider { get; init; }
    public AIModel UsedModel { get; init; }
    public bool IsSuccessful { get; init; }
    public string? ErrorMessage { get; init; }
    public string RevisedPrompt { get; init; } = string.Empty;
    public TimeSpan ResponseTime { get; init; }
}

public record AIHealthStatus
{
    public Dictionary<string, bool> ServiceStatus { get; init; } = new();
    public string RecommendedProvider { get; init; } = string.Empty;
    public bool FallbackAvailable { get; init; }
}

public record DemandForecast
{
    public int ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public int DaysPredicted { get; init; }
    public List<DemandPrediction> Predictions { get; init; } = new();
    public double ConfidenceScore { get; init; }
    public string TrendAnalysis { get; init; } = string.Empty;
    public List<string> Recommendations { get; init; } = new();
    public DateTime GeneratedAt { get; init; } = DateTime.UtcNow;
}

public record DemandPrediction
{
    public DateTime Date { get; init; }
    public double PredictedQuantity { get; init; }
    public double LowerBound { get; init; }
    public double UpperBound { get; init; }
    public double Confidence { get; init; }
    public List<string> InfluencingFactors { get; init; } = new();
}

public record InventoryOptimization
{
    public List<InventoryRecommendation> Recommendations { get; init; } = new();
    public double OptimizationScore { get; init; }
    public string Summary { get; init; } = string.Empty;
    public List<string> RiskFactors { get; init; } = new();
    public Dictionary<string, object> Metrics { get; init; } = new();
    public DateTime GeneratedAt { get; init; } = DateTime.UtcNow;
}

public record InventoryRecommendation
{
    public int ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public string Action { get; init; } = string.Empty;
    public int RecommendedQuantity { get; init; }
    public string Reason { get; init; } = string.Empty;
    public string Priority { get; init; } = string.Empty;
    public double EstimatedImpact { get; init; }
}

public record BusinessInsights
{
    public List<string> KeyInsights { get; init; } = new();
    public List<string> RecommendedActions { get; init; } = new();
    public string Summary { get; init; } = string.Empty;
    public double AnalysisScore { get; init; }
    public Dictionary<string, object> Metrics { get; init; } = new();
    public List<string> Trends { get; init; } = new();
    public List<string> Opportunities { get; init; } = new();
    public DateTime GeneratedAt { get; init; } = DateTime.UtcNow;
}

public record EnhancedOrderResult
{
    public List<EnhancedOrderItem> OrderItems { get; init; } = new();
    public bool IsValid { get; init; }
    public string ErrorMessage { get; init; } = string.Empty;
    public List<string> Warnings { get; init; } = new();
    public DateTime ProcessedAt { get; init; } = DateTime.UtcNow;
}

public record EnhancedOrderItem
{
    public int ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public DateTime? RequestedDate { get; init; }
    public string SpecialInstructions { get; init; } = string.Empty;
    public decimal EstimatedPrice { get; init; }
}

public record ProductAvailabilityValidation
{
    public List<ProductValidationResult> ValidationResults { get; init; } = new();
    public List<string> Recommendations { get; init; } = new();
    public bool AllProductsAvailable { get; init; }
    public string Summary { get; init; } = string.Empty;
    public DateTime ValidatedAt { get; init; } = DateTime.UtcNow;
}

public record ProductValidationResult
{
    public int ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public bool IsAvailable { get; init; }
    public bool IsActive { get; init; }
    public int RequestedQuantity { get; init; }
    public int AvailableStock { get; init; }
    public string ValidationMessage { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
}

public record AlternativeProductSuggestions
{
    public List<ProductAlternative> Suggestions { get; init; } = new();
    public double ConfidenceScore { get; init; }
    public string Summary { get; init; } = string.Empty;
    public List<string> GeneralRecommendations { get; init; } = new();
    public DateTime GeneratedAt { get; init; } = DateTime.UtcNow;
}

public record ProductAlternative
{
    public int OriginalProductId { get; init; }
    public string OriginalProductName { get; init; } = string.Empty;
    public int ReplacementProductId { get; init; }
    public string ReplacementProductName { get; init; } = string.Empty;
    public string Reason { get; init; } = string.Empty;
    public double SimilarityScore { get; init; }
    public decimal PriceDifference { get; init; }
    public bool IsInStock { get; init; }
}