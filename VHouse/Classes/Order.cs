namespace VHouse.Classes
{
    /// <summary>
    /// Represents an order with details about purchased products, pricing, and delivery.
    /// </summary>
    public class Order
    {
        /// <summary>
        /// Unique identifier for the order.
        /// </summary>
        public int OrderId { get; set; }

        /// <summary>
        /// The date and time when the order was placed.
        /// </summary>
        public DateTime OrderDate { get; set; } = DateTime.Now;

        /// <summary>
        /// The selected price type for this order (public, retail, or cost).
        /// </summary>
        public string PriceType { get; set; } = "public";

        /// <summary>
        /// The total amount of the order.
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// The applied discount.
        /// </summary>
        public decimal Discount { get; set; }

        /// <summary>
        /// Shipping cost for the order.
        /// </summary>
        public decimal ShippingCost { get; set; }

        /// <summary>
        /// Delivery date for the order.
        /// </summary> private DateTime _deliveryDate;
        private DateTime _deliveryDate;
        public DateTime DeliveryDate
        {
            get => _deliveryDate;
            set => _deliveryDate = DateTime.SpecifyKind(value, DateTimeKind.Utc); // ✅ Ensure it's UTC
        }

        /// <summary>
        /// The selected store for retail purchases (only applicable if price type is "retail").
        /// </summary>
        public string? SelectedStore { get; set; }

        /// <summary>
        /// List of products included in the order.
        /// </summary>
        public List<OrderItem> Items { get; set; } = new();
    }

    /// <summary>
    /// Represents a product within an order.
    /// </summary>
    public class OrderItem
    {
        public int OrderItemId { get; set; }
        public int OrderId { get; set; }  // Foreign Key
        public string ProductName { get; set; }
        public decimal Price { get; set; }
    }
}
