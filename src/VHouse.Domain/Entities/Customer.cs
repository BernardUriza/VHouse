using System.ComponentModel.DataAnnotations;

namespace VHouse.Domain.Entities;

public class Customer : BaseEntity
{
    [Required]
    [StringLength(200)]
    public string CustomerName { get; set; } = string.Empty;
    
    [StringLength(200)]
    public string Email { get; set; } = string.Empty;
    
    [StringLength(20)]
    public string Phone { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string Address { get; set; } = string.Empty;
    
    public bool IsVeganPreferred { get; set; } = true;
    
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public virtual ICollection<Order> Orders { get; } = new List<Order>();
}