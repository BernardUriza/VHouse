using Microsoft.EntityFrameworkCore;
using VHouse.Classes;
using VHouse.Interfaces;

namespace VHouse.Services
{
    /// <summary>
    /// Service for managing suppliers.
    /// </summary>
    public class SupplierService : ISupplierService
    {
        private readonly ApplicationDbContext _context;

        public SupplierService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Supplier>> GetSuppliersAsync()
        {
            return await _context.Suppliers
                .Include(s => s.PurchaseOrders)
                .Include(s => s.Products)
                .ToListAsync();
        }

        public async Task<Supplier?> GetSupplierByIdAsync(int supplierId)
        {
            return await _context.Suppliers
                .Include(s => s.PurchaseOrders)
                .Include(s => s.Products)
                .FirstOrDefaultAsync(s => s.SupplierId == supplierId);
        }

        public async Task<List<Supplier>> GetActiveSuppliersAsync()
        {
            return await _context.Suppliers
                .Where(s => s.IsActive)
                .Include(s => s.Products)
                .ToListAsync();
        }

        public async Task AddSupplierAsync(Supplier supplier)
        {
            _context.Suppliers.Add(supplier);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateSupplierAsync(Supplier supplier)
        {
            _context.Suppliers.Update(supplier);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteSupplierAsync(int supplierId)
        {
            var supplier = await _context.Suppliers.FindAsync(supplierId);
            if (supplier != null)
            {
                supplier.IsActive = false;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> SupplierExistsAsync(int supplierId)
        {
            return await _context.Suppliers.AnyAsync(s => s.SupplierId == supplierId);
        }
    }
}