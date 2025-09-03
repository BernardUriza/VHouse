using System.ComponentModel.DataAnnotations;

namespace VHouse.Domain.Entities;

/// <summary>
/// Consignación - Productos dejados en tienda para vender
/// Bernard deja productos en tiendas y cobra cuando se vendan
/// </summary>
public class Consignment
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string ConsignmentNumber { get; set; } = string.Empty; // CONS-2024-001
    
    [Required]
    public int ClientTenantId { get; set; }
    public virtual ClientTenant ClientTenant { get; set; } = null!;
    
    public DateTime ConsignmentDate { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiryDate { get; set; } // Fecha límite para devolver productos no vendidos
    
    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = ConsignmentStatus.Active;
    
    [MaxLength(500)]
    public string Notes { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string Terms { get; set; } = string.Empty; // Términos del acuerdo
    
    public decimal TotalValueAtCost { get; set; } // Valor total a costo de Bernard
    public decimal TotalValueAtRetail { get; set; } // Valor total al precio de venta
    
    // Porcentajes de ganancia
    public decimal StorePercentage { get; set; } = 30; // % que se queda la tienda
    public decimal BernardPercentage { get; set; } = 70; // % que recibe Bernard
    
    // Tracking financiero
    public decimal TotalSold { get; set; } = 0; // Total vendido por la tienda
    public decimal AmountDueToBernard { get; set; } = 0; // Lo que le deben a Bernard
    public decimal AmountDueToStore { get; set; } = 0; // Lo que se queda la tienda
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Items en consignación
    public virtual ICollection<ConsignmentItem> ConsignmentItems { get; set; } = new List<ConsignmentItem>();
    
    // Ventas registradas
    public virtual ICollection<ConsignmentSale> ConsignmentSales { get; set; } = new List<ConsignmentSale>();
}

/// <summary>
/// Item específico en consignación
/// </summary>
public class ConsignmentItem
{
    [Key]
    public int Id { get; set; }
    
    public int ConsignmentId { get; set; }
    public virtual Consignment Consignment { get; set; } = null!;
    
    public int ProductId { get; set; }
    public virtual Product Product { get; set; } = null!;
    
    public int QuantityConsigned { get; set; } // Cantidad dejada
    public int QuantitySold { get; set; } // Cantidad vendida
    public int QuantityReturned { get; set; } // Cantidad devuelta
    
    public decimal CostPrice { get; set; } // Precio de costo para Bernard
    public decimal RetailPrice { get; set; } // Precio de venta al público
    
    [MaxLength(200)]
    public string? Notes { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Cantidad disponible = Consignada - Vendida - Devuelta
    public int QuantityAvailable => QuantityConsigned - QuantitySold - QuantityReturned;
}

/// <summary>
/// Venta individual de un item en consignación
/// </summary>
public class ConsignmentSale
{
    [Key]
    public int Id { get; set; }
    
    public int ConsignmentId { get; set; }
    public virtual Consignment Consignment { get; set; } = null!;
    
    public int ConsignmentItemId { get; set; }
    public virtual ConsignmentItem ConsignmentItem { get; set; } = null!;
    
    public DateTime SaleDate { get; set; } = DateTime.UtcNow;
    
    public int QuantitySold { get; set; }
    public decimal UnitPrice { get; set; } // Precio al que se vendió
    public decimal TotalSaleAmount { get; set; }
    
    public decimal StoreAmount { get; set; } // Lo que se queda la tienda
    public decimal BernardAmount { get; set; } // Lo que recibe Bernard
    
    [MaxLength(200)]
    public string? SaleReference { get; set; } // Referencia de la venta de la tienda
    
    [MaxLength(500)]
    public string? Notes { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Estados de consignación
/// </summary>
public static class ConsignmentStatus
{
    public const string Active = "Active";           // Activa - productos en tienda
    public const string PartiallySettled = "PartiallySettled"; // Parcialmente liquidada
    public const string Settled = "Settled";         // Liquidada completamente
    public const string Expired = "Expired";         // Vencida - productos a devolver
    public const string Returned = "Returned";       // Productos devueltos
    public const string Cancelled = "Cancelled";     // Cancelada
}

/// <summary>
/// Extensiones para estados de consignación
/// </summary>
public static class ConsignmentStatusExtensions
{
    public static string ToSpanish(this string status)
    {
        return status switch
        {
            ConsignmentStatus.Active => "Activa",
            ConsignmentStatus.PartiallySettled => "Parcialmente Liquidada",
            ConsignmentStatus.Settled => "Liquidada",
            ConsignmentStatus.Expired => "Vencida",
            ConsignmentStatus.Returned => "Devuelta",
            ConsignmentStatus.Cancelled => "Cancelada",
            _ => status
        };
    }
    
    public static string GetEmoji(this string status)
    {
        return status switch
        {
            ConsignmentStatus.Active => "🔄",
            ConsignmentStatus.PartiallySettled => "💰",
            ConsignmentStatus.Settled => "✅",
            ConsignmentStatus.Expired => "⏰",
            ConsignmentStatus.Returned => "📦",
            ConsignmentStatus.Cancelled => "❌",
            _ => "📋"
        };
    }
}