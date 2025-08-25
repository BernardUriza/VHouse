using Microsoft.EntityFrameworkCore;
using VHouse.Classes;
using VHouse.Interfaces;

namespace VHouse.Services
{
    /// <summary>
    /// Service for managing purchase orders.
    /// </summary>
    public class PurchaseOrderService : IPurchaseOrderService
    {
        private readonly ApplicationDbContext _context;

        public PurchaseOrderService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<PurchaseOrder>> GetPurchaseOrdersAsync()
        {
            return await _context.PurchaseOrders
                .Include(po => po.Supplier)
                .Include(po => po.Warehouse)
                .Include(po => po.Items)
                    .ThenInclude(poi => poi.Product)
                .OrderByDescending(po => po.OrderDate)
                .ToListAsync();
        }

        public async Task<PurchaseOrder?> GetPurchaseOrderByIdAsync(int purchaseOrderId)
        {
            return await _context.PurchaseOrders
                .Include(po => po.Supplier)
                .Include(po => po.Warehouse)
                .Include(po => po.Items)
                    .ThenInclude(poi => poi.Product)
                .FirstOrDefaultAsync(po => po.PurchaseOrderId == purchaseOrderId);
        }

        public async Task<List<PurchaseOrder>> GetPurchaseOrdersBySupplierAsync(int supplierId)
        {
            return await _context.PurchaseOrders
                .Where(po => po.SupplierId == supplierId)
                .Include(po => po.Items)
                    .ThenInclude(poi => poi.Product)
                .OrderByDescending(po => po.OrderDate)
                .ToListAsync();
        }

        public async Task<List<PurchaseOrder>> GetPurchaseOrdersByStatusAsync(string status)
        {
            return await _context.PurchaseOrders
                .Where(po => po.Status == status)
                .Include(po => po.Supplier)
                .Include(po => po.Items)
                .OrderByDescending(po => po.OrderDate)
                .ToListAsync();
        }

        public async Task<PurchaseOrder> CreatePurchaseOrderAsync(PurchaseOrder purchaseOrder)
        {
            if (string.IsNullOrEmpty(purchaseOrder.OrderNumber))
            {
                purchaseOrder.OrderNumber = await GenerateOrderNumberAsync();
            }

            purchaseOrder.CalculateTotals();
            
            _context.PurchaseOrders.Add(purchaseOrder);
            await _context.SaveChangesAsync();
            
            return purchaseOrder;
        }

        public async Task UpdatePurchaseOrderAsync(PurchaseOrder purchaseOrder)
        {
            purchaseOrder.CalculateTotals();
            _context.PurchaseOrders.Update(purchaseOrder);
            await _context.SaveChangesAsync();
        }

        public async Task DeletePurchaseOrderAsync(int purchaseOrderId)
        {
            var purchaseOrder = await _context.PurchaseOrders.FindAsync(purchaseOrderId);
            if (purchaseOrder != null && purchaseOrder.Status == "Draft")
            {
                _context.PurchaseOrders.Remove(purchaseOrder);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<string> GenerateOrderNumberAsync()
        {
            var year = DateTime.UtcNow.Year;
            var lastOrder = await _context.PurchaseOrders
                .Where(po => po.OrderNumber.StartsWith($"PO-{year}-"))
                .OrderByDescending(po => po.OrderNumber)
                .FirstOrDefaultAsync();

            int nextNumber = 1;
            if (lastOrder != null)
            {
                var parts = lastOrder.OrderNumber.Split('-');
                if (parts.Length >= 3 && int.TryParse(parts[2], out int lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            return $"PO-{year}-{nextNumber:D4}";
        }

        public async Task ReceivePurchaseOrderAsync(int purchaseOrderId, Dictionary<int, int> receivedQuantities)
        {
            var purchaseOrder = await GetPurchaseOrderByIdAsync(purchaseOrderId);
            if (purchaseOrder == null || purchaseOrder.Status != "Confirmed")
                throw new InvalidOperationException("Purchase order not found or not in confirmed status");

            foreach (var item in purchaseOrder.Items)
            {
                if (receivedQuantities.ContainsKey(item.PurchaseOrderItemId))
                {
                    item.QuantityReceived += receivedQuantities[item.PurchaseOrderItemId];
                }
            }

            if (purchaseOrder.Items.All(i => i.IsFullyReceived))
            {
                purchaseOrder.Status = "Delivered";
                purchaseOrder.ActualDeliveryDate = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }

        public async Task ApprovePurchaseOrderAsync(int purchaseOrderId)
        {
            var purchaseOrder = await _context.PurchaseOrders.FindAsync(purchaseOrderId);
            if (purchaseOrder != null && purchaseOrder.Status == "Draft")
            {
                purchaseOrder.Status = "Confirmed";
                await _context.SaveChangesAsync();
            }
        }

        public async Task CancelPurchaseOrderAsync(int purchaseOrderId)
        {
            var purchaseOrder = await _context.PurchaseOrders.FindAsync(purchaseOrderId);
            if (purchaseOrder != null && (purchaseOrder.Status == "Draft" || purchaseOrder.Status == "Confirmed"))
            {
                purchaseOrder.Status = "Cancelled";
                await _context.SaveChangesAsync();
            }
        }
    }
}