namespace VHouse.Application.DTOs;

public class ConsignmentDto
{
    public int Id { get; set; }
    public string ConsignmentNumber { get; set; } = string.Empty;
    public int ClientTenantId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public DateTime ConsignmentDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string StatusSpanish { get; set; } = string.Empty;
    public string StatusEmoji { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public string Terms { get; set; } = string.Empty;

    public decimal TotalValueAtCost { get; set; }
    public decimal TotalValueAtRetail { get; set; }
    public decimal StorePercentage { get; set; }
    public decimal BernardPercentage { get; set; }

    public decimal TotalSold { get; set; }
    public decimal AmountDueToBernard { get; set; }
    public decimal AmountDueToStore { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public List<ConsignmentItemDto> Items { get; set; } = new();
    public List<ConsignmentSaleDto> Sales { get; set; } = new();
}

public class ConsignmentItemDto
{
    public int Id { get; set; }
    public int ConsignmentId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductEmoji { get; set; } = string.Empty;

    public int QuantityConsigned { get; set; }
    public int QuantitySold { get; set; }
    public int QuantityReturned { get; set; }
    public int QuantityAvailable { get; set; }

    public decimal CostPrice { get; set; }
    public decimal RetailPrice { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ConsignmentSaleDto
{
    public int Id { get; set; }
    public int ConsignmentId { get; set; }
    public int ConsignmentItemId { get; set; }
    public string ProductName { get; set; } = string.Empty;

    public DateTime SaleDate { get; set; }
    public int QuantitySold { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalSaleAmount { get; set; }
    public decimal StoreAmount { get; set; }
    public decimal BernardAmount { get; set; }

    public string? SaleReference { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}
