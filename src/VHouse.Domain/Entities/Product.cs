using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VHouse.Domain.Entities;

public class Product : BaseEntity
{
    [Required]
    [StringLength(200)]
    public string ProductName { get; set; } = string.Empty;
    
    [StringLength(10)]
    public string Emoji { get; set; } = string.Empty;
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal PriceCost { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal PriceRetail { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal PriceSuggested { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal PricePublic { get; set; }
    
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;
    
    public int StockQuantity { get; set; }
    
    public bool IsVegan { get; set; } = true;
    
    public bool IsActive { get; set; } = true;
    
    // Recommendation scoring for Phase 2 AI features
    public double Score { get; set; } = 0.0;
    
    // Navigation properties
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}