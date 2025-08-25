using System.ComponentModel.DataAnnotations;

namespace VHouse.Classes
{
    /// <summary>
    /// Represents a product brand in the B2B system.
    /// </summary>
    public class Brand
    {
        /// <summary>
        /// Unique identifier for the brand.
        /// </summary>
        [Key]
        public int BrandId { get; set; }

        /// <summary>
        /// Name of the brand.
        /// </summary>
        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Description of the brand.
        /// </summary>
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Brand logo URL or path.
        /// </summary>
        [StringLength(255)]
        public string LogoUrl { get; set; } = string.Empty;

        /// <summary>
        /// Website URL of the brand.
        /// </summary>
        [StringLength(255)]
        public string Website { get; set; } = string.Empty;

        /// <summary>
        /// Indicates if the brand is currently active.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Date when the brand was created.
        /// </summary>
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Products associated with this brand.
        /// </summary>
        public List<Product> Products { get; set; } = new();
    }
}