using System.ComponentModel.DataAnnotations;

namespace VHouse.Domain.Entities;

/// <summary>
/// Entrega realizada a un cliente
/// </summary>
public class Delivery
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public int OrderId { get; set; }
    public virtual Order Order { get; set; } = null!;
    
    [Required]
    public int ClientTenantId { get; set; }
    public virtual ClientTenant ClientTenant { get; set; } = null!;
    
    [Required]
    [MaxLength(50)]
    public string DeliveryNumber { get; set; } = string.Empty; // DEL-2024-001
    
    public DateTime DeliveryDate { get; set; } = DateTime.UtcNow;
    
    public DateTime? PlannedDeliveryDate { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = DeliveryStatus.Pending;
    
    [MaxLength(200)]
    public string DeliveryAddress { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string ContactPerson { get; set; } = string.Empty;
    
    [MaxLength(20)]
    public string ContactPhone { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string DeliveryNotes { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string InternalNotes { get; set; } = string.Empty; // Para Bernard
    
    public decimal TotalAmount { get; set; }
    
    // Campos para tracking
    public DateTime? DepartureTime { get; set; }
    public DateTime? ArrivalTime { get; set; }
    public DateTime? CompletionTime { get; set; }
    
    [MaxLength(100)]
    public string? ReceivedBy { get; set; } // Quién recibió la entrega
    
    [MaxLength(500)]
    public string? DeliveryFeedback { get; set; } // Comentarios del cliente
    
    // Para futuro: GPS tracking
    public decimal? DeliveryLatitude { get; set; }
    public decimal? DeliveryLongitude { get; set; }
    
    // Fotos de entrega (URLs)
    [MaxLength(1000)]
    public string? DeliveryPhotoUrls { get; set; } // JSON array de URLs
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Items entregados
    public virtual ICollection<DeliveryItem> DeliveryItems { get; set; } = new List<DeliveryItem>();
}

/// <summary>
/// Items específicos entregados
/// </summary>
public class DeliveryItem
{
    [Key]
    public int Id { get; set; }
    
    public int DeliveryId { get; set; }
    public virtual Delivery Delivery { get; set; } = null!;
    
    public int ProductId { get; set; }
    public virtual Product Product { get; set; } = null!;
    
    public int QuantityDelivered { get; set; }
    public int QuantityOrdered { get; set; }
    
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    
    [MaxLength(200)]
    public string? Notes { get; set; } // Ej: "Entregado en buen estado", "Faltó 1 unidad"
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Estados de entrega
/// </summary>
public static class DeliveryStatus
{
    public const string Pending = "Pending";           // Pendiente
    public const string Preparing = "Preparing";       // Preparando
    public const string InTransit = "InTransit";       // En camino
    public const string Delivered = "Delivered";       // Entregado
    public const string PartiallyDelivered = "PartiallyDelivered"; // Entrega parcial
    public const string Failed = "Failed";             // Falló la entrega
    public const string Cancelled = "Cancelled";       // Cancelado
}

/// <summary>
/// Extensión para obtener el nombre en español
/// </summary>
public static class DeliveryStatusExtensions
{
    public static string ToSpanish(this string status)
    {
        return status switch
        {
            DeliveryStatus.Pending => "Pendiente",
            DeliveryStatus.Preparing => "Preparando",
            DeliveryStatus.InTransit => "En Camino",
            DeliveryStatus.Delivered => "Entregado",
            DeliveryStatus.PartiallyDelivered => "Entrega Parcial",
            DeliveryStatus.Failed => "Falló",
            DeliveryStatus.Cancelled => "Cancelado",
            _ => status
        };
    }
    
    public static string GetEmoji(this string status)
    {
        return status switch
        {
            DeliveryStatus.Pending => "⏳",
            DeliveryStatus.Preparing => "📦",
            DeliveryStatus.InTransit => "🚚",
            DeliveryStatus.Delivered => "✅",
            DeliveryStatus.PartiallyDelivered => "⚠️",
            DeliveryStatus.Failed => "❌",
            DeliveryStatus.Cancelled => "🚫",
            _ => "📋"
        };
    }
}