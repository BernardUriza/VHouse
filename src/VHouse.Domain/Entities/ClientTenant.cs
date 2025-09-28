using System.ComponentModel.DataAnnotations;

namespace VHouse.Domain.Entities;

public class ClientTenant : BaseEntity
{
    
    [Required]
    [MaxLength(100)]
    public string TenantName { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(50)]
    public string TenantCode { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string BusinessName { get; set; } = string.Empty;
    
    [MaxLength(200)]
    public string Description { get; set; } = string.Empty;
    
    [MaxLength(50)]
    public string ContactPerson { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string Email { get; set; } = string.Empty;
    
    [MaxLength(20)]
    public string Phone { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(50)]
    public string LoginUsername { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(255)]
    public string LoginPasswordHash { get; set; } = string.Empty;
    
    public bool IsActive { get; set; } = true;
    
    public new DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? LastLoginAt { get; set; }
    
    // Navigation properties
    public virtual ICollection<ClientProduct> ClientProducts { get; } = new List<ClientProduct>();
    public virtual ICollection<Order> Orders { get; } = new List<Order>();
}

// Tabla intermedia para productos asignados por cliente
public class ClientProduct
{
    [Key]
    public int Id { get; set; }
    
    public int ClientTenantId { get; set; }
    public virtual ClientTenant ClientTenant { get; set; } = null!;
    
    public int ProductId { get; set; }
    public virtual Product Product { get; set; } = null!;
    
    public decimal CustomPrice { get; set; }
    public int MinOrderQuantity { get; set; } = 1;
    public bool IsAvailable { get; set; } = true;
    
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
}