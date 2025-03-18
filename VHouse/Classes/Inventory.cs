namespace VHouse.Classes
{
    public class Inventory
    {
        public int InventoryId { get; set; }
        public int CustomerId { get; set; }
        public Customer Customer { get; set; } = null!;
        public bool IsGeneralInventory { get; set; }
        public List<InventoryItem> Items { get; set; } = new();
    }

    public class InventoryItem
    {
        public int InventoryItemId { get; set; }
        public int InventoryId { get; set; }
        public Inventory Inventory { get; set; } = null!;

        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        public int Quantity { get; set; } = 0;

        public DateTime ExpirationDate { get; set; }  // 📆 Se requiere para rastrear el vencimiento
        public int InvoiceId { get; set; }
        public Invoice Invoice { get; set; }  // 🔗 Relación con la factura de compra
    }

}
