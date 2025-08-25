using Microsoft.EntityFrameworkCore;
using VHouse.Classes;
using VHouse.Interfaces;

namespace VHouse.Services
{
    /// <summary>
    /// Service for managing warehouses and warehouse inventory.
    /// </summary>
    public class WarehouseService : IWarehouseService
    {
        private readonly ApplicationDbContext _context;

        public WarehouseService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Warehouse>> GetWarehousesAsync()
        {
            return await _context.Warehouses
                .Include(w => w.PurchaseOrders)
                .Include(w => w.InventoryItems)
                    .ThenInclude(wi => wi.Product)
                .OrderBy(w => w.Name)
                .ToListAsync();
        }

        public async Task<Warehouse?> GetWarehouseByIdAsync(int warehouseId)
        {
            return await _context.Warehouses
                .Include(w => w.PurchaseOrders)
                .Include(w => w.InventoryItems)
                    .ThenInclude(wi => wi.Product)
                .FirstOrDefaultAsync(w => w.WarehouseId == warehouseId);
        }

        public async Task<List<Warehouse>> GetActiveWarehousesAsync()
        {
            return await _context.Warehouses
                .Where(w => w.IsActive)
                .OrderBy(w => w.Name)
                .ToListAsync();
        }

        public async Task<Warehouse?> GetDefaultWarehouseAsync()
        {
            return await _context.Warehouses
                .FirstOrDefaultAsync(w => w.IsDefault && w.IsActive);
        }

        public async Task AddWarehouseAsync(Warehouse warehouse)
        {
            // Ensure warehouse code is unique
            var existingWarehouse = await _context.Warehouses
                .FirstOrDefaultAsync(w => w.Code == warehouse.Code);
            
            if (existingWarehouse != null)
                throw new InvalidOperationException($"Warehouse with code '{warehouse.Code}' already exists.");

            // If this is the first warehouse, make it default
            if (!await _context.Warehouses.AnyAsync())
            {
                warehouse.IsDefault = true;
            }

            _context.Warehouses.Add(warehouse);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateWarehouseAsync(Warehouse warehouse)
        {
            _context.Warehouses.Update(warehouse);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteWarehouseAsync(int warehouseId)
        {
            var warehouse = await _context.Warehouses.FindAsync(warehouseId);
            if (warehouse != null)
            {
                warehouse.IsActive = false;
                
                // If this was the default warehouse, set another active warehouse as default
                if (warehouse.IsDefault)
                {
                    var newDefault = await _context.Warehouses
                        .Where(w => w.IsActive && w.WarehouseId != warehouseId)
                        .FirstOrDefaultAsync();
                    
                    if (newDefault != null)
                    {
                        newDefault.IsDefault = true;
                    }
                }
                
                await _context.SaveChangesAsync();
            }
        }

        public async Task SetDefaultWarehouseAsync(int warehouseId)
        {
            var warehouse = await _context.Warehouses.FindAsync(warehouseId);
            if (warehouse != null && warehouse.IsActive)
            {
                // Remove default flag from all warehouses
                await _context.Warehouses
                    .Where(w => w.IsDefault)
                    .ExecuteUpdateAsync(w => w.SetProperty(p => p.IsDefault, false));

                // Set the new default
                warehouse.IsDefault = true;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<WarehouseInventory>> GetWarehouseInventoryAsync(int warehouseId)
        {
            return await _context.WarehouseInventories
                .Where(wi => wi.WarehouseId == warehouseId)
                .Include(wi => wi.Product)
                .OrderBy(wi => wi.Product != null ? wi.Product.ProductName : string.Empty)
                .ToListAsync();
        }

        public async Task UpdateInventoryAsync(int warehouseId, int productId, int quantity)
        {
            var inventory = await _context.WarehouseInventories
                .FirstOrDefaultAsync(wi => wi.WarehouseId == warehouseId && wi.ProductId == productId);

            if (inventory == null)
            {
                inventory = new WarehouseInventory
                {
                    WarehouseId = warehouseId,
                    ProductId = productId,
                    QuantityOnHand = quantity,
                    LastUpdated = DateTime.UtcNow
                };
                _context.WarehouseInventories.Add(inventory);
            }
            else
            {
                inventory.QuantityOnHand = quantity;
                inventory.LastUpdated = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }

        public async Task TransferInventoryAsync(int fromWarehouseId, int toWarehouseId, int productId, int quantity)
        {
            var fromInventory = await _context.WarehouseInventories
                .FirstOrDefaultAsync(wi => wi.WarehouseId == fromWarehouseId && wi.ProductId == productId);

            if (fromInventory == null || fromInventory.AvailableQuantity < quantity)
                throw new InvalidOperationException("Insufficient inventory for transfer.");

            var toInventory = await _context.WarehouseInventories
                .FirstOrDefaultAsync(wi => wi.WarehouseId == toWarehouseId && wi.ProductId == productId);

            // Reduce from source warehouse
            fromInventory.QuantityOnHand -= quantity;
            fromInventory.LastUpdated = DateTime.UtcNow;

            // Add to destination warehouse
            if (toInventory == null)
            {
                toInventory = new WarehouseInventory
                {
                    WarehouseId = toWarehouseId,
                    ProductId = productId,
                    QuantityOnHand = quantity,
                    LastUpdated = DateTime.UtcNow
                };
                _context.WarehouseInventories.Add(toInventory);
            }
            else
            {
                toInventory.QuantityOnHand += quantity;
                toInventory.LastUpdated = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }
    }
}