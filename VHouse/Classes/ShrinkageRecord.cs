using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VHouse.Classes
{
    /// <summary>
    /// Represents a shrinkage record for inventory loss tracking.
    /// </summary>
    public class ShrinkageRecord
    {
        /// <summary>
        /// Unique identifier for the shrinkage record.
        /// </summary>
        [Key]
        public int ShrinkageRecordId { get; set; }

        /// <summary>
        /// Foreign key to the product affected by shrinkage.
        /// </summary>
        [ForeignKey("Product")]
        public int ProductId { get; set; }

        /// <summary>
        /// Foreign key to the warehouse where shrinkage occurred.
        /// </summary>
        [ForeignKey("Warehouse")]
        public int? WarehouseId { get; set; }

        /// <summary>
        /// Quantity lost due to shrinkage.
        /// </summary>
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Shrinkage quantity must be at least 1.")]
        public int QuantityLost { get; set; }

        /// <summary>
        /// Reason for shrinkage.
        /// </summary>
        [Required, StringLength(50)]
        public string Reason { get; set; } = string.Empty; // Damage, Theft, Expiration, Administrative Error, etc.

        /// <summary>
        /// Detailed description of the shrinkage incident.
        /// </summary>
        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Date when shrinkage was discovered.
        /// </summary>
        public DateTime DiscoveryDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Date when shrinkage was recorded in the system.
        /// </summary>
        public DateTime RecordDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// User who recorded the shrinkage.
        /// </summary>
        [StringLength(100)]
        public string RecordedBy { get; set; } = string.Empty;

        /// <summary>
        /// Cost impact of the shrinkage (calculated from product cost).
        /// </summary>
        [Column(TypeName = "decimal(10,2)")]
        public decimal CostImpact { get; set; }

        /// <summary>
        /// Reference number for the shrinkage report.
        /// </summary>
        [StringLength(50)]
        public string ReferenceNumber { get; set; } = string.Empty;

        /// <summary>
        /// Indicates if this shrinkage has been approved.
        /// </summary>
        public bool IsApproved { get; set; } = false;

        /// <summary>
        /// User who approved the shrinkage record.
        /// </summary>
        [StringLength(100)]
        public string ApprovedBy { get; set; } = string.Empty;

        /// <summary>
        /// Date when shrinkage was approved.
        /// </summary>
        public DateTime? ApprovalDate { get; set; }

        /// <summary>
        /// Navigation property to product.
        /// </summary>
        public Product? Product { get; set; }

        /// <summary>
        /// Navigation property to warehouse.
        /// </summary>
        public Warehouse? Warehouse { get; set; }
    }

    /// <summary>
    /// Enumeration of common shrinkage reasons.
    /// </summary>
    public static class ShrinkageReasons
    {
        public const string Damage = "Damage";
        public const string Theft = "Theft";
        public const string Expiration = "Expiration";
        public const string AdministrativeError = "Administrative Error";
        public const string CountDiscrepancy = "Count Discrepancy";
        public const string QualityIssue = "Quality Issue";
        public const string Other = "Other";
    }
}