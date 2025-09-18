using System.ComponentModel.DataAnnotations;

namespace VHouse.Domain.Entities;

public class Album : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Slug { get; set; } = string.Empty;

    public string? Description { get; set; }

    // Navigation properties
    public virtual ICollection<Photo> Photos { get; } = new List<Photo>();
}