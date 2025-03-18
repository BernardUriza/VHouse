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
        public string Emoji { get; set; }

        /// <summary>
        /// Name of the product.
        /// </summary>
        public string ProductName { get; set; }

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
    }
}
