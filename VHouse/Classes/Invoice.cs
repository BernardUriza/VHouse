namespace VHouse.Classes
{
    public class Invoice
    {
        public int InvoiceId { get; set; }
        public string ProviderName { get; set; } = string.Empty;  // 📦 Proveedor o tienda donde se compró
        public DateTime InvoiceDate { get; set; } = DateTime.UtcNow;
        public List<InventoryItem> Items { get; set; } = new();
    }

}
