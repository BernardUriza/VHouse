using Microsoft.EntityFrameworkCore;
using VHouse.Domain.Entities;
using VHouse.Domain.Interfaces;
using VHouse.Infrastructure.Data;

namespace VHouse.Infrastructure.Repositories;

public class OrderRepository : Repository<Order>, IOrderRepository
{
    public OrderRepository(VHouseDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Order>> GetOrdersByCustomerAsync(int customerId)
    {
        return await _dbSet.Where(o => o.CustomerId == customerId)
                          .AsNoTracking()
                          .ToListAsync();
    }

    public async Task<IEnumerable<Order>> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _dbSet.Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate)
                          .AsNoTracking()
                          .ToListAsync();
    }

    public async Task<IEnumerable<Order>> GetRecentOrdersByCustomerAsync(int customerId, int days)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-days);
        return await _dbSet.Where(o => o.CustomerId == customerId && o.OrderDate >= cutoffDate)
                          .AsNoTracking()
                          .ToListAsync();
    }

    public async Task<int> GetOrderCountByCustomerAsync(int customerId)
    {
        return await _dbSet.CountAsync(o => o.CustomerId == customerId);
    }
}