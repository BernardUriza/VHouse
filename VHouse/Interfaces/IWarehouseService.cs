using VHouse.Classes;

namespace VHouse.Interfaces
{
    /// <summary>
    /// Service interface for warehouse management.
    /// </summary>
    public interface IWarehouseService
    {
        Task<List<Warehouse>> GetWarehousesAsync();
        Task<Warehouse?> GetWarehouseByIdAsync(int warehouseId);
        Task<List<Warehouse>> GetActiveWarehousesAsync();
        Task<Warehouse?> GetDefaultWarehouseAsync();
        Task AddWarehouseAsync(Warehouse warehouse);
        Task UpdateWarehouseAsync(Warehouse warehouse);
        Task DeleteWarehouseAsync(int warehouseId);
        Task SetDefaultWarehouseAsync(int warehouseId);
        Task<List<WarehouseInventory>> GetWarehouseInventoryAsync(int warehouseId);
        Task UpdateInventoryAsync(int warehouseId, int productId, int quantity);
        Task TransferInventoryAsync(int fromWarehouseId, int toWarehouseId, int productId, int quantity);
    }
}