using Microsoft.EntityFrameworkCore;
using VHouse.Domain.Entities;
using VHouse.Domain.Interfaces;
using VHouse.Infrastructure.Data;

namespace VHouse.Infrastructure.Repositories;

public class SupplierRepository : Repository<Supplier>, ISupplierRepository
{
    public SupplierRepository(VHouseDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Supplier>> GetActiveSuppliers()
    {
        return await _dbSet
            .Where(s => s.IsActive)
            .OrderBy(s => s.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Supplier>> GetVeganCertifiedSuppliers()
    {
        return await _dbSet
            .Where(s => s.IsActive && s.IsVeganCertified)
            .OrderBy(s => s.Name)
            .ToListAsync();
    }

    public async Task<Supplier?> GetSupplierWithProducts(int id)
    {
        // TODO: Uncomment when Product.SupplierId is added
        // return await _dbSet
        //     .Include(s => s.Products)
        //     .FirstOrDefaultAsync(s => s.Id == id);
        return await _dbSet.FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<bool> HasActiveProducts(int supplierId)
    {
        // TODO: Uncomment when SupplierId is added to Product entity
        // return await _context.Set<Product>()
        //     .AnyAsync(p => p.SupplierId == supplierId && p.IsActive);
        return false; // Temporary: no products linked to suppliers yet
    }
}