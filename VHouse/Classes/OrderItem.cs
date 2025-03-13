using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VHouse.Classes
{
    /// <summary>
    /// Represents a product within an order.
    /// </summary>
    public class OrderItem
    {
        [Key]
        public int OrderItemId { get; set; } // Primary Key

        [ForeignKey("Order")]
        public int OrderId { get; set; } // Foreign Key to Order

        [ForeignKey("Product")]
        public int ProductId { get; set; } // Foreign Key to Product

        [Required]
        public string ProductName { get; set; } = string.Empty; // Store Product Name (for historical records)

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than zero.")]
        public decimal Price { get; set; } // Price at the time of order

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; } // ✅ New field: Quantity purchased

        // Navigation properties
        public Order? Order { get; set; }
        public Product? Product { get; set; }

        /// <summary>
        /// Calculates the total price for this order item.
        /// </summary>
        public decimal TotalPrice => Price * Quantity;
    }
}
