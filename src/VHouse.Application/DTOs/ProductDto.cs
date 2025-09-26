namespace VHouse.Application.DTOs;

public class ProductDto
{
    public int Id { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Emoji { get; set; } = string.Empty;
    public decimal PriceCost { get; set; }
    public decimal PriceRetail { get; set; }
    public decimal PriceSuggested { get; set; }
    public decimal PricePublic { get; set; }
    public string Description { get; set; } = string.Empty;
    public int StockQuantity { get; set; }
    public bool IsVegan { get; set; }
    public bool IsActive { get; set; }
}