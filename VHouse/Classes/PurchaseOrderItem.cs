using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VHouse.Classes
{
    /// <summary>
    /// Represents an item within a purchase order.
    /// </summary>
    public class PurchaseOrderItem
    {
        /// <summary>
        /// Unique identifier for the purchase order item.
        /// </summary>
        [Key]
        public int PurchaseOrderItemId { get; set; }

        /// <summary>
        /// Foreign key to the purchase order.
        /// </summary>
        [ForeignKey("PurchaseOrder")]
        public int PurchaseOrderId { get; set; }

        /// <summary>
        /// Foreign key to the product.
        /// </summary>
        [ForeignKey("Product")]
        public int ProductId { get; set; }

        /// <summary>
        /// Product name at the time of order (for historical records).
        /// </summary>
        [Required, StringLength(200)]
        public string ProductName { get; set; } = string.Empty;

        /// <summary>
        /// Product SKU at the time of order.
        /// </summary>
        [StringLength(100)]
        public string ProductSku { get; set; } = string.Empty;

        /// <summary>
        /// Unit cost from supplier.
        /// </summary>
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Unit cost must be greater than zero.")]
        public decimal UnitCost { get; set; }

        /// <summary>
        /// Quantity ordered.
        /// </summary>
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int QuantityOrdered { get; set; }

        /// <summary>
        /// Quantity received (for partial deliveries).
        /// </summary>
        public int QuantityReceived { get; set; } = 0;

        /// <summary>
        /// Notes specific to this item.
        /// </summary>
        [StringLength(500)]
        public string Notes { get; set; } = string.Empty;

        /// <summary>
        /// Navigation property to purchase order.
        /// </summary>
        public PurchaseOrder? PurchaseOrder { get; set; }

        /// <summary>
        /// Navigation property to product.
        /// </summary>
        public Product? Product { get; set; }

        /// <summary>
        /// Calculates the total cost for this item.
        /// </summary>
        public decimal TotalPrice => UnitCost * QuantityOrdered;

        /// <summary>
        /// Indicates if the full quantity has been received.
        /// </summary>
        public bool IsFullyReceived => QuantityReceived >= QuantityOrdered;
    }
}