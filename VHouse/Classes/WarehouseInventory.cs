using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VHouse.Classes
{
    /// <summary>
    /// Represents inventory levels for products at specific warehouses.
    /// </summary>
    public class WarehouseInventory
    {
        /// <summary>
        /// Unique identifier for the warehouse inventory record.
        /// </summary>
        [Key]
        public int WarehouseInventoryId { get; set; }

        /// <summary>
        /// Foreign key to the warehouse.
        /// </summary>
        [ForeignKey("Warehouse")]
        public int WarehouseId { get; set; }

        /// <summary>
        /// Foreign key to the product.
        /// </summary>
        [ForeignKey("Product")]
        public int ProductId { get; set; }

        /// <summary>
        /// Current quantity in stock at this warehouse.
        /// </summary>
        [Required]
        public int QuantityOnHand { get; set; }

        /// <summary>
        /// Minimum stock level threshold for reorder alerts.
        /// </summary>
        public int MinimumStock { get; set; } = 0;

        /// <summary>
        /// Maximum stock level for this product at this warehouse.
        /// </summary>
        public int MaximumStock { get; set; } = int.MaxValue;

        /// <summary>
        /// Reserved quantity (allocated to orders but not yet shipped).
        /// </summary>
        public int ReservedQuantity { get; set; } = 0;

        /// <summary>
        /// Location within the warehouse (aisle, shelf, etc.).
        /// </summary>
        [StringLength(100)]
        public string Location { get; set; } = string.Empty;

        /// <summary>
        /// Last date when inventory was updated.
        /// </summary>
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Last date when physical count was performed.
        /// </summary>
        public DateTime? LastCountDate { get; set; }

        /// <summary>
        /// Navigation property to warehouse.
        /// </summary>
        public Warehouse? Warehouse { get; set; }

        /// <summary>
        /// Navigation property to product.
        /// </summary>
        public Product? Product { get; set; }

        /// <summary>
        /// Available quantity (on hand minus reserved).
        /// </summary>
        public int AvailableQuantity => QuantityOnHand - ReservedQuantity;

        /// <summary>
        /// Indicates if stock is below minimum threshold.
        /// </summary>
        public bool IsLowStock => QuantityOnHand <= MinimumStock;

        /// <summary>
        /// Indicates if product is out of stock.
        /// </summary>
        public bool IsOutOfStock => QuantityOnHand <= 0;
    }
}