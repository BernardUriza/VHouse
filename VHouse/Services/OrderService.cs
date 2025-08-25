using Microsoft.EntityFrameworkCore;
using VHouse.Classes;
using VHouse.Interfaces;

namespace VHouse.Services;

/// <summary>
/// Service for managing orders, inventory, and product metrics.
/// </summary>
public class OrderService : IOrderService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<OrderService> _logger;

    public OrderService(ApplicationDbContext context, ILogger<OrderService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves all orders with their items.
    /// </summary>
    public async Task<List<Order>> GetOrdersAsync()
    {
        return await _context.Orders
            .Include(o => o.Items)
            .ToListAsync();
    }

    /// <summary>
    /// Deletes an order by ID.
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
    /// Processes and persists a full order including inventory and scoring logic.
    /// </summary>
    public async Task<bool> ProcessOrderAsync(Order order)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            _logger.LogInformation("🔄 Processing order {OrderId}...", order.OrderId);

            // Save order first
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            if (order.IsInventoryEntry)
                await ProcessInventoryEntryAsync(order);

            await UpdateProductScoresAsync(order.Items);

            if (order.CustomerId.HasValue)
            {
                var customer = await _context.Customers.FindAsync(order.CustomerId.Value);
                if (customer?.IsRetail == true)
                    await UpdateCustomerInventoryAsync(customer.CustomerId, order.Items);
            }

            await transaction.CommitAsync();
            _logger.LogInformation("✅ Order {OrderId} processed successfully.", order.OrderId);
            return true;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "❌ Error en ProcessOrderAsync");
            return false;
        }
    }

    /// <summary>
    /// Updates general inventory from an invoice entry.
    /// </summary>
    private async Task ProcessInventoryEntryAsync(Order order)
    {
        var generalInventory = await _context.Inventories
            .Include(i => i.Items)
            .FirstOrDefaultAsync(i => i.IsGeneralInventory);

        if (generalInventory == null)
        {
            generalInventory = new Inventory { IsGeneralInventory = true };
            _context.Inventories.Add(generalInventory);
            await _context.SaveChangesAsync();
        }

        foreach (var item in order.Items)
        {
            var existing = generalInventory.Items.FirstOrDefault(i => i.ProductId == item.ProductId);
            if (existing != null)
            {
                existing.Quantity += item.Quantity;
            }
            else
            {
                generalInventory.Items.Add(new InventoryItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    ExpirationDate = DateTime.UtcNow.AddMonths(6)
                });
            }
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("📦 General inventory updated for order {OrderId}", order.OrderId);
    }

    /// <summary>
    /// Increases score on each product ordered.
    /// </summary>
    private async Task UpdateProductScoresAsync(List<OrderItem> items)
    {
        foreach (var item in items)
        {
            var product = await _context.Products.FindAsync(item.ProductId);
            if (product != null)
                product.Score += item.Quantity;
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("🌟 Product scores updated.");
    }

    /// <summary>
    /// Updates a customer's inventory based on ordered items.
    /// </summary>
    private async Task UpdateCustomerInventoryAsync(int customerId, List<OrderItem> items)
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
            var existing = inventory.Items.FirstOrDefault(i => i.ProductId == item.ProductId);
            if (existing != null)
            {
                existing.Quantity += item.Quantity;
            }
            else
            {
                inventory.Items.Add(new InventoryItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity
                });
            }
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("🏬 Customer {CustomerId} inventory updated.", customerId);
    }
}
