using System.ComponentModel.DataAnnotations;

namespace VHouse.Domain.Entities;

public class Supplier : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(200)]
    public string? ContactName { get; set; }
    
    [EmailAddress]
    [MaxLength(100)]
    public string? Email { get; set; }
    
    [Phone]
    [MaxLength(20)]
    public string? Phone { get; set; }
    
    [MaxLength(500)]
    public string? Address { get; set; }
    
    [MaxLength(100)]
    public string? City { get; set; }
    
    [MaxLength(20)]
    public string? PostalCode { get; set; }
    
    [MaxLength(100)]
    public string? Country { get; set; }
    
    [MaxLength(1000)]
    public string? Notes { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    // Vegan certification info
    public bool IsVeganCertified { get; set; }
    
    [MaxLength(100)]
    public string? CertificationNumber { get; set; }
    
    public DateTime? CertificationExpiry { get; set; }
    
    // Payment terms
    [MaxLength(100)]
    public string? PaymentTerms { get; set; }
    
    public decimal? MinimumOrderAmount { get; set; }
    
    // Navigation properties
    public virtual ICollection<Product> Products { get; } = new List<Product>();
}