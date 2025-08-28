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
        Products = new Repository<Product>(_context);
        Customers = new Repository<Customer>(_context);
        Orders = new Repository<Order>(_context);
        OrderItems = new Repository<OrderItem>(_context);
    }

    public IRepository<Product> Products { get; private set; }
    public IRepository<Customer> Customers { get; private set; }
    public IRepository<Order> Orders { get; private set; }
    public IRepository<OrderItem> OrderItems { get; private set; }

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
    }
}