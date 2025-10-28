using Microsoft.EntityFrameworkCore;
using VHouse.Domain.Entities;
using VHouse.Domain.Interfaces;
using VHouse.Infrastructure.Data;

namespace VHouse.Infrastructure.Repositories;

public class ConsignmentRepository : IConsignmentRepository
{
    private readonly VHouseDbContext _context;

    public ConsignmentRepository(VHouseDbContext context)
    {
        _context = context;
    }

    public async Task<List<Consignment>> GetAllAsync()
    {
        return await _context.Consignments
            .Include(c => c.ClientTenant)
            .Include(c => c.ConsignmentItems)
                .ThenInclude(ci => ci.Product)
            .Include(c => c.ConsignmentSales)
                .ThenInclude(cs => cs.ConsignmentItem)
                    .ThenInclude(ci => ci.Product)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<Consignment?> GetByIdAsync(int id)
    {
        return await _context.Consignments
            .Include(c => c.ClientTenant)
            .Include(c => c.ConsignmentItems)
                .ThenInclude(ci => ci.Product)
            .Include(c => c.ConsignmentSales)
                .ThenInclude(cs => cs.ConsignmentItem)
                    .ThenInclude(ci => ci.Product)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Consignment> AddAsync(Consignment consignment)
    {
        _context.Consignments.Add(consignment);
        await _context.SaveChangesAsync();
        return consignment;
    }

    public async Task UpdateAsync(Consignment consignment)
    {
        consignment.UpdatedAt = DateTime.UtcNow;
        _context.Consignments.Update(consignment);
        await _context.SaveChangesAsync();
    }

    public async Task<int> GetCountAsync()
    {
        return await _context.Consignments.CountAsync();
    }

    public async Task<Consignment?> GetByIdWithAllDetailsAsync(int id)
    {
        return await _context.Consignments
            .Include(c => c.ClientTenant)
            .Include(c => c.ConsignmentItems)
                .ThenInclude(ci => ci.Product)
            .Include(c => c.ConsignmentSales)
                .ThenInclude(cs => cs.ConsignmentItem)
                    .ThenInclude(ci => ci.Product)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Consignment?> GetByIdForSettlementAsync(int id)
    {
        return await _context.Consignments
            .Include(c => c.ClientTenant)
            .Include(c => c.ConsignmentItems)
                .ThenInclude(ci => ci.Product)
            .Include(c => c.ConsignmentSales)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task AddConsignmentItemsAsync(IEnumerable<ConsignmentItem> items)
    {
        await _context.ConsignmentItems.AddRangeAsync(items);
    }

    public async Task<ConsignmentItem?> GetConsignmentItemByIdAsync(int itemId)
    {
        return await _context.ConsignmentItems
            .Include(ci => ci.Product)
            .Include(ci => ci.Consignment)
            .FirstOrDefaultAsync(ci => ci.Id == itemId);
    }

    public async Task<ConsignmentSale> AddConsignmentSaleAsync(ConsignmentSale sale)
    {
        _context.ConsignmentSales.Add(sale);
        return sale;
    }

    public async Task<List<ConsignmentSale>> GetSalesByConsignmentIdAsync(int consignmentId)
    {
        return await _context.ConsignmentSales
            .Include(cs => cs.ConsignmentItem)
                .ThenInclude(ci => ci.Product)
            .Where(cs => cs.ConsignmentId == consignmentId)
            .OrderByDescending(cs => cs.SaleDate)
            .ToListAsync();
    }
}
