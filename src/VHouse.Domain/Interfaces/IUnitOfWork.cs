using VHouse.Domain.Entities;

namespace VHouse.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IProductRepository Products { get; }
    ICustomerRepository Customers { get; }
    IOrderRepository Orders { get; }
    IRepository<OrderItem> OrderItems { get; }
    IRepository<ClientTenant> ClientTenants { get; }
    IRepository<PriceList> PriceLists { get; }
    IRepository<PriceListItem> PriceListItems { get; }
    IRepository<ClientTenantPriceList> ClientTenantPriceLists { get; }
    IConsignmentRepository Consignments { get; }
    // NOTE: ConsignmentItem and ConsignmentSale are managed via IConsignmentRepository methods
    // They cannot use IRepository<T> because they don't inherit from BaseEntity

    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
