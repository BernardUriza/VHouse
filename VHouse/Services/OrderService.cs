using Microsoft.EntityFrameworkCore;
using VHouse.Classes;

namespace VHouse.Services
{
    public class OrderService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<OrderService> _logger;

        public OrderService(ApplicationDbContext context, ILogger<OrderService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves all orders from the database.
        /// </summary>
        public async Task<List<Order>> GetOrdersAsync()
        {
            return await _context.Orders
                .Include(o => o.Items)
                .ToListAsync();
        }

        /// <summary>
        /// Saves a new order to the database.
        /// </summary>
        public async Task SaveOrderAsync(Order order)
        {
            order.OrderDate = DateTime.UtcNow;
            order.DeliveryDate = order.DeliveryDate.ToUniversalTime();

            foreach (var item in order.Items)
            {
                item.Product = null; // ✅ Prevents EF from re-inserting Products
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

        /// <summary>
        /// Processes an order, handling inventory updates and popularity scores.
        /// </summary>
        public async Task<bool> ProcessOrderAsync(Order order)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                _logger.LogInformation("🔄 Procesando pedido {OrderId}...", order.OrderId);

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                if (order.IsInventoryEntry)
                {
                    await ProcessInventoryEntry(order);
                }

                await UpdateProductScores(order.Items);

                if (order.CustomerId.HasValue)
                {
                    var customer = await _context.Customers.FindAsync(order.CustomerId.Value);
                    if (customer?.IsRetail == true)
                    {
                        await UpdateCustomerInventory(customer.CustomerId, order.Items);
                    }
                }

                await transaction.CommitAsync();
                _logger.LogInformation("✅ Pedido {OrderId} procesado correctamente.", order.OrderId);
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError($"❌ Error en ProcessOrderAsync: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Processes an inventory entry when an order is marked as an inventory addition.
        /// </summary>
        private async Task ProcessInventoryEntry(Order order)
        {
            var generalInventory = await _context.Inventories.FirstOrDefaultAsync(i => i.IsGeneralInventory);

            if (generalInventory == null)
            {
                generalInventory = new Inventory { IsGeneralInventory = true };
                _context.Inventories.Add(generalInventory);
                await _context.SaveChangesAsync();
            }

            foreach (var item in order.Items)
            {
                var inventoryItem = await _context.InventoryItems
                    .FirstOrDefaultAsync(i => i.ProductId == item.ProductId && i.InventoryId == generalInventory.InventoryId);

                if (inventoryItem == null)
                {
                    _context.InventoryItems.Add(new InventoryItem
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        ExpirationDate = DateTime.UtcNow.AddMonths(6),
                        InvoiceId = order.OrderId,
                        InventoryId = generalInventory.InventoryId
                    });
                }
                else
                {
                    inventoryItem.Quantity += item.Quantity;
                }
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("📦 Inventario actualizado para pedido {OrderId}", order.OrderId);
        }

        /// <summary>
        /// Updates product popularity scores based on order items.
        /// </summary>
        private async Task UpdateProductScores(List<OrderItem> items)
        {
            foreach (var item in items)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product != null)
                {
                    product.Score += item.Quantity;
                }
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("🌟 Puntuaciones de productos actualizadas.");
        }

        /// <summary>
        /// Updates a customer's inventory when they receive a retail order.
        /// </summary>
        private async Task UpdateCustomerInventory(int customerId, List<OrderItem> items)
        {
            var inventory = await _context.Inventories
                .Include(i => i.Items)
                .FirstOrDefaultAsync(i => i.CustomerId == customerId);

            if (inventory == null)
            {
                inventory = new Inventory { CustomerId = customerId };
                _context.Inventories.Add(inventory);
                await _context.SaveChangesAsync();
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
            _logger.LogInformation("🏬 Inventario de cliente {CustomerId} actualizado.", customerId);
        }
    }
}
