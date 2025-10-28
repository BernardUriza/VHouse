using VHouse.Domain.Entities;

namespace VHouse.Domain.Interfaces;

public interface IConsignmentRepository
{
    // Existing methods
    Task<List<Consignment>> GetAllAsync();
    Task<Consignment?> GetByIdAsync(int id);
    Task<Consignment> AddAsync(Consignment consignment);
    Task UpdateAsync(Consignment consignment);
    Task<int> GetCountAsync();

    // Specialized query methods
    Task<Consignment?> GetByIdWithAllDetailsAsync(int id);
    Task<Consignment?> GetByIdForSettlementAsync(int id);

    // Item operations
    Task AddConsignmentItemsAsync(IEnumerable<ConsignmentItem> items);
    Task<ConsignmentItem?> GetConsignmentItemByIdAsync(int itemId);

    // Sale operations
    Task<ConsignmentSale> AddConsignmentSaleAsync(ConsignmentSale sale);
    Task<List<ConsignmentSale>> GetSalesByConsignmentIdAsync(int consignmentId);
}
