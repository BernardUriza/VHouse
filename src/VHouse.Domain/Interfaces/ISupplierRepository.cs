using VHouse.Domain.Entities;

namespace VHouse.Domain.Interfaces;

public interface ISupplierRepository : IRepository<Supplier>
{
    Task<IEnumerable<Supplier>> GetActiveSuppliers();
    Task<IEnumerable<Supplier>> GetVeganCertifiedSuppliers();
    Task<Supplier?> GetSupplierWithProducts(int id);
    Task<bool> HasActiveProducts(int supplierId);
}