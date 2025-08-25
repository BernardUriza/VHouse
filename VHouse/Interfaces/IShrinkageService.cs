using VHouse.Classes;

namespace VHouse.Interfaces
{
    /// <summary>
    /// Service interface for shrinkage management.
    /// </summary>
    public interface IShrinkageService
    {
        Task<List<ShrinkageRecord>> GetShrinkageRecordsAsync();
        Task<ShrinkageRecord?> GetShrinkageRecordByIdAsync(int shrinkageRecordId);
        Task<List<ShrinkageRecord>> GetShrinkageRecordsByProductAsync(int productId);
        Task<List<ShrinkageRecord>> GetShrinkageRecordsByWarehouseAsync(int warehouseId);
        Task<List<ShrinkageRecord>> GetShrinkageRecordsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<ShrinkageRecord> CreateShrinkageRecordAsync(ShrinkageRecord shrinkageRecord);
        Task UpdateShrinkageRecordAsync(ShrinkageRecord shrinkageRecord);
        Task ApproveShrinkageRecordAsync(int shrinkageRecordId, string approvedBy);
        Task<decimal> GetTotalShrinkageCostAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<Dictionary<string, int>> GetShrinkageByReasonAsync(DateTime? startDate = null, DateTime? endDate = null);
    }
}