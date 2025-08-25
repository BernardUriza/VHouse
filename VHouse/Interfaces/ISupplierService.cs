using VHouse.Classes;

namespace VHouse.Interfaces
{
    /// <summary>
    /// Service interface for supplier management.
    /// </summary>
    public interface ISupplierService
    {
        Task<List<Supplier>> GetSuppliersAsync();
        Task<Supplier?> GetSupplierByIdAsync(int supplierId);
        Task<List<Supplier>> GetActiveSuppliersAsync();
        Task AddSupplierAsync(Supplier supplier);
        Task UpdateSupplierAsync(Supplier supplier);
        Task DeleteSupplierAsync(int supplierId);
        Task<bool> SupplierExistsAsync(int supplierId);
    }
}