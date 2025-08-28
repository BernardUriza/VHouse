namespace VHouse.Application.DTOs;

public record ProductDto
{
    public int Id { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public string Emoji { get; init; } = string.Empty;
    public decimal PriceCost { get; init; }
    public decimal PriceRetail { get; init; }
    public decimal PriceSuggested { get; init; }
    public decimal PricePublic { get; init; }
    public string Description { get; init; } = string.Empty;
    public int StockQuantity { get; init; }
    public bool IsVegan { get; init; }
    public bool IsActive { get; init; }
}