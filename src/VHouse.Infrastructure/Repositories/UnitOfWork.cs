using Microsoft.EntityFrameworkCore.Storage;
using VHouse.Domain.Entities;
using VHouse.Domain.Interfaces;
using VHouse.Infrastructure.Data;

namespace VHouse.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly VHouseDbContext _context;
    private IDbContextTransaction? _transaction;

    public UnitOfWork(VHouseDbContext context)
    {
        _context = context;
        Products = new ProductRepository(_context);
        Customers = new CustomerRepository(_context);
        Orders = new OrderRepository(_context);
        OrderItems = new Repository<OrderItem>(_context);
        ClientTenants = new Repository<ClientTenant>(_context);
        PriceLists = new Repository<PriceList>(_context);
        PriceListItems = new Repository<PriceListItem>(_context);
        ClientTenantPriceLists = new Repository<ClientTenantPriceList>(_context);
        Consignments = new ConsignmentRepository(_context);
    }

    public IProductRepository Products { get; private set; }
    public ICustomerRepository Customers { get; private set; }
    public IOrderRepository Orders { get; private set; }
    public IRepository<OrderItem> OrderItems { get; private set; }
    public IRepository<ClientTenant> ClientTenants { get; private set; }
    public IRepository<PriceList> PriceLists { get; private set; }
    public IRepository<PriceListItem> PriceListItems { get; private set; }
    public IRepository<ClientTenantPriceList> ClientTenantPriceLists { get; private set; }
    public IConsignmentRepository Consignments { get; private set; }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}