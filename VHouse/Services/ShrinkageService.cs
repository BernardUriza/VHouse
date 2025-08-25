using Microsoft.EntityFrameworkCore;
using VHouse.Classes;
using VHouse.Interfaces;

namespace VHouse.Services
{
    /// <summary>
    /// Service for managing shrinkage records.
    /// </summary>
    public class ShrinkageService : IShrinkageService
    {
        private readonly ApplicationDbContext _context;

        public ShrinkageService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<ShrinkageRecord>> GetShrinkageRecordsAsync()
        {
            return await _context.ShrinkageRecords
                .Include(sr => sr.Product)
                .Include(sr => sr.Warehouse)
                .OrderByDescending(sr => sr.RecordDate)
                .ToListAsync();
        }

        public async Task<ShrinkageRecord?> GetShrinkageRecordByIdAsync(int shrinkageRecordId)
        {
            return await _context.ShrinkageRecords
                .Include(sr => sr.Product)
                .Include(sr => sr.Warehouse)
                .FirstOrDefaultAsync(sr => sr.ShrinkageRecordId == shrinkageRecordId);
        }

        public async Task<List<ShrinkageRecord>> GetShrinkageRecordsByProductAsync(int productId)
        {
            return await _context.ShrinkageRecords
                .Where(sr => sr.ProductId == productId)
                .Include(sr => sr.Product)
                .Include(sr => sr.Warehouse)
                .OrderByDescending(sr => sr.RecordDate)
                .ToListAsync();
        }

        public async Task<List<ShrinkageRecord>> GetShrinkageRecordsByWarehouseAsync(int warehouseId)
        {
            return await _context.ShrinkageRecords
                .Where(sr => sr.WarehouseId == warehouseId)
                .Include(sr => sr.Product)
                .Include(sr => sr.Warehouse)
                .OrderByDescending(sr => sr.RecordDate)
                .ToListAsync();
        }

        public async Task<List<ShrinkageRecord>> GetShrinkageRecordsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.ShrinkageRecords
                .Where(sr => sr.DiscoveryDate >= startDate && sr.DiscoveryDate <= endDate)
                .Include(sr => sr.Product)
                .Include(sr => sr.Warehouse)
                .OrderByDescending(sr => sr.RecordDate)
                .ToListAsync();
        }

        public async Task<ShrinkageRecord> CreateShrinkageRecordAsync(ShrinkageRecord shrinkageRecord)
        {
            // Calculate cost impact based on product cost
            var product = await _context.Products.FindAsync(shrinkageRecord.ProductId);
            if (product != null)
            {
                shrinkageRecord.CostImpact = product.PriceCost * shrinkageRecord.QuantityLost;
            }

            // Generate reference number if not provided
            if (string.IsNullOrEmpty(shrinkageRecord.ReferenceNumber))
            {
                shrinkageRecord.ReferenceNumber = await GenerateReferenceNumberAsync();
            }

            // Update warehouse inventory if warehouse is specified
            if (shrinkageRecord.WarehouseId.HasValue)
            {
                var warehouseInventory = await _context.WarehouseInventories
                    .FirstOrDefaultAsync(wi => wi.WarehouseId == shrinkageRecord.WarehouseId.Value 
                                            && wi.ProductId == shrinkageRecord.ProductId);

                if (warehouseInventory != null)
                {
                    warehouseInventory.QuantityOnHand = Math.Max(0, 
                        warehouseInventory.QuantityOnHand - shrinkageRecord.QuantityLost);
                    warehouseInventory.LastUpdated = DateTime.UtcNow;
                }
            }

            _context.ShrinkageRecords.Add(shrinkageRecord);
            await _context.SaveChangesAsync();
            
            return shrinkageRecord;
        }

        public async Task UpdateShrinkageRecordAsync(ShrinkageRecord shrinkageRecord)
        {
            _context.ShrinkageRecords.Update(shrinkageRecord);
            await _context.SaveChangesAsync();
        }

        public async Task ApproveShrinkageRecordAsync(int shrinkageRecordId, string approvedBy)
        {
            var shrinkageRecord = await _context.ShrinkageRecords.FindAsync(shrinkageRecordId);
            if (shrinkageRecord != null && !shrinkageRecord.IsApproved)
            {
                shrinkageRecord.IsApproved = true;
                shrinkageRecord.ApprovedBy = approvedBy;
                shrinkageRecord.ApprovalDate = DateTime.UtcNow;
                
                await _context.SaveChangesAsync();
            }
        }

        public async Task<decimal> GetTotalShrinkageCostAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.ShrinkageRecords.AsQueryable();

            if (startDate.HasValue)
                query = query.Where(sr => sr.DiscoveryDate >= startDate.Value);
            
            if (endDate.HasValue)
                query = query.Where(sr => sr.DiscoveryDate <= endDate.Value);

            return await query.SumAsync(sr => sr.CostImpact);
        }

        public async Task<Dictionary<string, int>> GetShrinkageByReasonAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.ShrinkageRecords.AsQueryable();

            if (startDate.HasValue)
                query = query.Where(sr => sr.DiscoveryDate >= startDate.Value);
            
            if (endDate.HasValue)
                query = query.Where(sr => sr.DiscoveryDate <= endDate.Value);

            return await query
                .GroupBy(sr => sr.Reason)
                .Select(g => new { Reason = g.Key, Total = g.Sum(sr => sr.QuantityLost) })
                .ToDictionaryAsync(x => x.Reason, x => x.Total);
        }

        private async Task<string> GenerateReferenceNumberAsync()
        {
            var year = DateTime.UtcNow.Year;
            var lastRecord = await _context.ShrinkageRecords
                .Where(sr => sr.ReferenceNumber.StartsWith($"SHR-{year}-"))
                .OrderByDescending(sr => sr.ReferenceNumber)
                .FirstOrDefaultAsync();

            int nextNumber = 1;
            if (lastRecord != null)
            {
                var parts = lastRecord.ReferenceNumber.Split('-');
                if (parts.Length >= 3 && int.TryParse(parts[2], out int lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            return $"SHR-{year}-{nextNumber:D4}";
        }
    }
}