using System.ComponentModel.DataAnnotations.Schema;

namespace VHouse.Domain.Entities;

public class OrderItem : BaseEntity
{
    public int OrderId { get; set; }
    
    public int ProductId { get; set; }
    
    public int Quantity { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal UnitPrice { get; set; }
    
    [NotMapped]
    public decimal TotalPrice => Quantity * UnitPrice;
    
    // Navigation properties
    public virtual Order Order { get; set; } = null!;
    public virtual Product Product { get; set; } = null!;
}