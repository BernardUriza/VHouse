using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using VHouse.Classes;

namespace VHouse
{
    /// <summary>
    /// Represents a product with multiple pricing options.
    /// </summary>
    public class Product
    {
        /// <summary>
        /// Unique identifier for the product. 
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// Emoji representation of the product for UI display.
        /// </summary>
        public string Emoji { get; set; } = string.Empty;

        /// <summary>
        /// Name of the product.
        /// </summary>
        [Required, StringLength(200)]
        public string ProductName { get; set; } = string.Empty;

        /// <summary>
        /// Product SKU (Stock Keeping Unit).
        /// </summary>
        [StringLength(100)]
        public string SKU { get; set; } = string.Empty;

        /// <summary>
        /// Product barcode.
        /// </summary>
        [StringLength(50)]
        public string Barcode { get; set; } = string.Empty;

        /// <summary>
        /// Product description.
        /// </summary>
        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Cost price (how much it costs to acquire or produce the product).
        /// </summary>
        public decimal PriceCost { get; set; }

        /// <summary>
        /// Retail price (price at which the product is sold in stores).
        /// </summary>
        public decimal PriceRetail { get; set; }

        /// <summary>
        /// Suggested price (maximum recommended retail price).
        /// </summary>
        public decimal PriceSuggested { get; set; }

        /// <summary>
        /// Activism price (special price for community or activism purposes).
        /// </summary>
        public decimal PricePublic { get; set; }

        /// <summary>
        /// Nuevo campo: mide la popularidad del producto
        /// </summary>
        public int Score { get; set; } = 0;

        /// <summary>
        /// Foreign key to the brand.
        /// </summary>
        [ForeignKey("Brand")]
        public int? BrandId { get; set; }

        /// <summary>
        /// Foreign key to the primary supplier.
        /// </summary>
        [ForeignKey("Supplier")]
        public int? SupplierId { get; set; }

        /// <summary>
        /// Indicates if the product is currently active.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Date when the product was created.
        /// </summary>
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Navigation property to brand.
        /// </summary>
        public Brand? Brand { get; set; }

        /// <summary>
        /// Navigation property to supplier.
        /// </summary>
        public Supplier? Supplier { get; set; }

        /// <summary>
        /// Warehouse inventory records for this product.
        /// </summary>
        public List<WarehouseInventory> WarehouseInventories { get; set; } = new();

        /// <summary>
        /// Purchase order items for this product.
        /// </summary>
        public List<PurchaseOrderItem> PurchaseOrderItems { get; set; } = new();

        /// <summary>
        /// Shrinkage records for this product.
        /// </summary>
        public List<ShrinkageRecord> ShrinkageRecords { get; set; } = new();
    }
}
