using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VHouse.Domain.Entities;

public class SystemMetric : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string MetricName { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(50)]
    public string MetricType { get; set; } = string.Empty; // PERFORMANCE, BUSINESS, SECURITY, INVENTORY
    
    [Column(TypeName = "decimal(18,4)")]
    public decimal Value { get; set; }
    
    [MaxLength(20)]
    public string Unit { get; set; } = string.Empty;
    
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    [MaxLength(100)]
    public string? Source { get; set; }
    
    [MaxLength(100)]
    public string? ClientTenant { get; set; }
    
    public string? Metadata { get; set; } // JSON with additional data
    
    [MaxLength(20)]
    public string Severity { get; set; } = "NORMAL"; // CRITICAL, WARNING, NORMAL, INFO
    
    public bool RequiresAction { get; set; } = false;
    
    [MaxLength(500)]
    public string? ActionRequired { get; set; }
}

public class BusinessAlert : BaseEntity
{
    [Required]
    [MaxLength(20)]
    public string AlertType { get; set; } = string.Empty; // STOCK_LOW, SALES_HIGH, CLIENT_INACTIVE, PERFORMANCE
    
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(20)]
    public string Severity { get; set; } = string.Empty; // CRITICAL, HIGH, MEDIUM, LOW
    
    [MaxLength(100)]
    public string? ClientTenant { get; set; }
    
    [MaxLength(100)]
    public string? RelatedEntity { get; set; }
    
    public int? RelatedEntityId { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal? AmountInvolved { get; set; }
    
    
    public DateTime? ResolvedAt { get; set; }
    
    public bool IsResolved { get; set; } = false;
    
    [MaxLength(200)]
    public string? ResolvedBy { get; set; }
    
    [MaxLength(500)]
    public string? ResolutionNotes { get; set; }
    
    public string? ActionData { get; set; } // JSON with action parameters
}