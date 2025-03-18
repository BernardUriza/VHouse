using Microsoft.EntityFrameworkCore;
using VHouse.Classes;

namespace VHouse.Services
{
    /// <summary>
    /// Service for managing orders and saving them in the database.
    /// </summary>
    public class OrderService
    {
        private readonly ApplicationDbContext _context;

        public OrderService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves all orders from the database.
        /// </summary>
        public async Task<List<Order>> GetOrdersAsync()
        {
            return await _context.Orders.Include(o => o.Items).ToListAsync();
        }

        /// <summary>
        /// Saves a new order to the database.
        /// </summary>
        public async Task SaveOrderAsync(Order order)
        {
            // ✅ Ensure DateTime fields are stored in UTC
            order.OrderDate = DateTime.UtcNow;
            order.DeliveryDate = order.DeliveryDate.ToUniversalTime();

            foreach (var item in order.Items)
            {
                item.Product = null; // ✅ Prevents EF from trying to re-insert Products
            }
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Deletes an order from the database.
        /// </summary>
        public async Task DeleteOrderAsync(int orderId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order != null)
            {
                _context.Orders.Remove(order);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ProcessOrderAsync(Order order)
        {
            if (order.IsInventoryEntry)
            {
                foreach (var item in order.Items)
                {
                    var inventoryItem = new InventoryItem
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        ExpirationDate = DateTime.UtcNow.AddMonths(6),  // 📆 Default a 6 meses
                        InvoiceId = order.OrderId  // 🔗 Relacionado con la "factura"
                    };

                    _context.Inventory.Add(inventoryItem);
                }
            }

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            var customer = await _context.Customers.FindAsync(order.CustomerId);
            if (customer?.IsRetail == true)
            {
                await UpdateInventory(customer.CustomerId, order.Items);
            }

            return true;
        }

        private async Task UpdateInventory(int customerId, List<OrderItem> items)
        {
            var inventory = await _context.Inventories
                .Include(i => i.Items)
                .FirstOrDefaultAsync(i => i.CustomerId == customerId);

            if (inventory == null)
            {
                inventory = new Inventory { CustomerId = customerId };
                _context.Inventories.Add(inventory);
            }

            foreach (var item in items)
            {
                var inventoryItem = inventory.Items.FirstOrDefault(i => i.ProductId == item.ProductId);
                if (inventoryItem == null)
                {
                    inventory.Items.Add(new InventoryItem
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity
                    });
                }
                else
                {
                    inventoryItem.Quantity += item.Quantity;
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}
