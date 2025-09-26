namespace VHouse.Application.DTOs;

public class PriceListDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class PriceListItemDto
{
    public int Id { get; set; }
    public int PriceListId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal CustomPrice { get; set; }
    public decimal DiscountPercentage { get; set; }
    public int MinOrderQuantity { get; set; }
    public bool IsActive { get; set; }
}

public class ClientTenantPriceListDto
{
    public int Id { get; set; }
    public int ClientTenantId { get; set; }
    public string ClientTenantName { get; set; } = string.Empty;
    public int PriceListId { get; set; }
    public string PriceListName { get; set; } = string.Empty;
    public DateTime AssignedAt { get; set; }
    public bool IsActive { get; set; }
}