using Microsoft.EntityFrameworkCore;
using VHouse.Domain.Entities;
using VHouse.Domain.Interfaces;
using VHouse.Infrastructure.Data;

namespace VHouse.Infrastructure.Repositories;

public class ProductRepository : Repository<Product>, IProductRepository
{
    public ProductRepository(VHouseDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Product>> GetActiveProductsAsync()
    {
        return await _dbSet.Where(p => p.IsActive)
                          .AsNoTracking()
                          .ToListAsync();
    }

    public async Task<IEnumerable<Product>> GetProductsByScoreAsync(int minScore)
    {
        return await _dbSet.Where(p => p.Score >= minScore)
                          .AsNoTracking()
                          .ToListAsync();
    }
}