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

    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}