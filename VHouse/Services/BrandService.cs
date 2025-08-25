using Microsoft.EntityFrameworkCore;
using VHouse.Classes;
using VHouse.Interfaces;

namespace VHouse.Services
{
    /// <summary>
    /// Service for managing brands.
    /// </summary>
    public class BrandService : IBrandService
    {
        private readonly ApplicationDbContext _context;

        public BrandService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Brand>> GetBrandsAsync()
        {
            return await _context.Brands
                .Include(b => b.Products)
                .ToListAsync();
        }

        public async Task<Brand?> GetBrandByIdAsync(int brandId)
        {
            return await _context.Brands
                .Include(b => b.Products)
                .FirstOrDefaultAsync(b => b.BrandId == brandId);
        }

        public async Task<List<Brand>> GetActiveBrandsAsync()
        {
            return await _context.Brands
                .Where(b => b.IsActive)
                .Include(b => b.Products)
                .ToListAsync();
        }

        public async Task AddBrandAsync(Brand brand)
        {
            _context.Brands.Add(brand);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateBrandAsync(Brand brand)
        {
            _context.Brands.Update(brand);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteBrandAsync(int brandId)
        {
            var brand = await _context.Brands.FindAsync(brandId);
            if (brand != null)
            {
                brand.IsActive = false;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> BrandExistsAsync(int brandId)
        {
            return await _context.Brands.AnyAsync(b => b.BrandId == brandId);
        }
    }
}