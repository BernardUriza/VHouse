using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VHouse.Domain.Entities;

public class Photo : BaseEntity
{
    [Required]
    public int AlbumId { get; set; }

    [Required]
    [MaxLength(500)]
    public string FileName { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string OriginalName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string ContentType { get; set; } = string.Empty;

    public long SizeBytes { get; set; }

    public DateTime UploadedUtc { get; set; } = DateTime.UtcNow;

    [MaxLength(500)]
    public string? Caption { get; set; }

    [MaxLength(500)]
    public string? ThumbnailPath { get; set; }

    // Navigation properties
    [ForeignKey(nameof(AlbumId))]
    public virtual Album Album { get; set; } = null!;
}