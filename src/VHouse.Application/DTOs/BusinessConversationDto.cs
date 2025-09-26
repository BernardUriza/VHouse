using VHouse.Domain.Enums;

namespace VHouse.Application.DTOs;

public record BusinessConversationResponseDto
{
    public string Response { get; init; } = string.Empty;
    public string ConversationContext { get; init; } = string.Empty;
    public IReadOnlyCollection<BusinessAction> SuggestedActions { get; init; } = new List<BusinessAction>();
    public IReadOnlyCollection<ProductSuggestion> ProductRecommendations { get; init; } = new List<ProductSuggestion>();
    public AIProvider UsedProvider { get; init; }
    public string UsedModel { get; init; } = string.Empty;
    public bool IsSuccessful { get; init; }
    public string? ErrorMessage { get; init; }
    public double ResponseTimeMs { get; init; }
    public BusinessPriority Priority { get; init; }
    public IReadOnlyCollection<string> ExtractedEntities { get; init; } = new List<string>();
}

public record BusinessEmailResponseDto
{
    public string Subject { get; init; } = string.Empty;
    public string Body { get; init; } = string.Empty;
    public string EmailType { get; init; } = string.Empty;
    public bool IsUrgent { get; init; }
    public IReadOnlyCollection<string> RequiredAttachments { get; init; } = new List<string>();
    public AIProvider UsedProvider { get; init; }
    public bool IsSuccessful { get; init; }
    public string? ErrorMessage { get; init; }
}

public record ComplexOrderResponseDto
{
    public IReadOnlyCollection<OrderItem> ExtractedItems { get; init; } = new List<OrderItem>();
    public OrderSummary OrderSummary { get; init; } = new();
    public IReadOnlyCollection<BusinessAlert> Alerts { get; init; } = new List<BusinessAlert>();
    public IReadOnlyCollection<string> MissingInformation { get; init; } = new List<string>();
    public decimal EstimatedTotal { get; init; }
    public DateTime? RequestedDeliveryDate { get; init; }
    public PaymentTerms? PaymentTerms { get; init; }
    public AIProvider UsedProvider { get; init; }
    public bool IsSuccessful { get; init; }
    public string? ErrorMessage { get; init; }
}

public record BusinessAction
{
    public string ActionType { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public Uri? ActionUrl { get; init; }
    public BusinessPriority Priority { get; init; }
}

public record ProductSuggestion
{
    public int ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public string ReasonForSuggestion { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public bool InStock { get; init; }
}

public record OrderItem
{
    public int ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public string? SpecialInstructions { get; init; }
}

public record OrderSummary
{
    public int TotalItems { get; init; }
    public decimal SubTotal { get; init; }
    public decimal EstimatedTax { get; init; }
    public decimal EstimatedTotal { get; init; }
    public string Currency { get; init; } = "MXN";
}

public record BusinessAlert
{
    public string AlertType { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public BusinessPriority Priority { get; init; }
    public IReadOnlyCollection<string> SuggestedActions { get; init; } = new List<string>();
}

public record BusinessContext
{
    public string CustomerType { get; init; } = string.Empty;
    public IReadOnlyCollection<string> PreferredBrands { get; init; } = new List<string>();
    public IReadOnlyCollection<string> RecentOrderHistory { get; init; } = new List<string>();
    public decimal TypicalOrderValue { get; init; }
    public string PaymentHistory { get; init; } = string.Empty;
    public IReadOnlyDictionary<string, object> CustomData { get; init; } = new Dictionary<string, object>();
}

public record PaymentTerms
{
    public int DaysNet { get; init; }
    public decimal DiscountPercentage { get; init; }
    public int DiscountDays { get; init; }
    public string TermsDescription { get; init; } = string.Empty;
}