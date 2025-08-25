using System.ComponentModel.DataAnnotations;

namespace VHouse.Classes
{
    /// <summary>
    /// Represents a purchase order for B2B procurement.
    /// </summary>
    public class PurchaseOrder
    {
        /// <summary>
        /// Unique identifier for the purchase order.
        /// </summary>
        [Key]
        public int PurchaseOrderId { get; set; }

        /// <summary>
        /// Purchase order number (human-readable).
        /// </summary>
        [Required, StringLength(50)]
        public string OrderNumber { get; set; } = string.Empty;

        /// <summary>
        /// Date when the purchase order was created.
        /// </summary>
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Expected delivery date.
        /// </summary>
        public DateTime ExpectedDeliveryDate { get; set; }

        /// <summary>
        /// Actual delivery date (if delivered).
        /// </summary>
        public DateTime? ActualDeliveryDate { get; set; }

        /// <summary>
        /// Purchase order status.
        /// </summary>
        [Required, StringLength(50)]
        public string Status { get; set; } = "Draft"; // Draft, Sent, Confirmed, Delivered, Cancelled

        /// <summary>
        /// Total amount before taxes.
        /// </summary>
        public decimal SubTotal { get; set; }

        /// <summary>
        /// Tax amount.
        /// </summary>
        public decimal TaxAmount { get; set; }

        /// <summary>
        /// Total amount including taxes.
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Additional notes for the purchase order.
        /// </summary>
        [StringLength(1000)]
        public string Notes { get; set; } = string.Empty;

        /// <summary>
        /// Supplier who will fulfill this order.
        /// </summary>
        [Required]
        public int SupplierId { get; set; }
        public Supplier? Supplier { get; set; }

        /// <summary>
        /// Warehouse where goods will be received.
        /// </summary>
        public int? WarehouseId { get; set; }
        public Warehouse? Warehouse { get; set; }

        /// <summary>
        /// Items included in this purchase order.
        /// </summary>
        public List<PurchaseOrderItem> Items { get; set; } = new();

        /// <summary>
        /// Calculates total amount from items.
        /// </summary>
        public void CalculateTotals()
        {
            SubTotal = Items.Sum(item => item.TotalPrice);
            TotalAmount = SubTotal + TaxAmount;
        }
    }
}