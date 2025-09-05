using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VHouse.Domain.Entities;

public class AuditLog : BaseEntity
{
    [Required]
    [MaxLength(50)]
    public string Action { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string EntityType { get; set; } = string.Empty;
    
    public int? EntityId { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string UserId { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(200)]
    public string UserName { get; set; } = string.Empty;
    
    [MaxLength(50)]
    public string IPAddress { get; set; } = string.Empty;
    
    [MaxLength(200)]
    public string UserAgent { get; set; } = string.Empty;
    
    public string? OldValues { get; set; }
    
    public string? NewValues { get; set; }
    
    [MaxLength(500)]
    public string Changes { get; set; } = string.Empty;
    
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    [MaxLength(20)]
    public string Severity { get; set; } = "INFO";
    
    [MaxLength(50)]
    public string Module { get; set; } = string.Empty;
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal? AmountInvolved { get; set; }
    
    [MaxLength(100)]
    public string? ClientTenant { get; set; }
    
    public bool IsSuccess { get; set; } = true;
    
    [MaxLength(500)]
    public string? ErrorMessage { get; set; }
    
    public TimeSpan? ExecutionTime { get; set; }
}