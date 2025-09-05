using Microsoft.EntityFrameworkCore;
using VHouse.Domain.Entities;
using VHouse.Application.DTOs;
using VHouse.Application.Services;
using VHouse.Infrastructure.Data;

namespace VHouse.Infrastructure.Services;

public class CustomerService : ICustomerService
{
    private readonly VHouseDbContext _context;

    public CustomerService(VHouseDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Customer>> GetActiveCustomersAsync(int? clientTenantId = null)
    {
        var query = _context.Customers
            .Where(c => c.IsActive);

        if (clientTenantId.HasValue)
        {
            query = query.Where(c => c.Orders.Any(o => o.ClientTenantId == clientTenantId));
        }

        return await query
            .OrderBy(c => c.CustomerName)
            .ToListAsync();
    }

    public async Task<Customer?> GetCustomerByIdAsync(int customerId)
    {
        return await _context.Customers
            .Include(c => c.Orders)
            .FirstOrDefaultAsync(c => c.Id == customerId);
    }

    public async Task<Customer> CreateCustomerAsync(CreateCustomerDto dto, int? clientTenantId = null)
    {
        var customer = new Customer
        {
            CustomerName = dto.CustomerName,
            Email = dto.Email,
            Phone = dto.Phone,
            Address = dto.Address,
            IsVeganPreferred = dto.IsVeganPreferred,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();
        return customer;
    }

    public async Task<Customer> UpdateCustomerAsync(int customerId, UpdateCustomerDto dto)
    {
        var customer = await _context.Customers.FindAsync(customerId);
        if (customer == null)
            throw new ArgumentException($"Customer with ID {customerId} not found");

        customer.CustomerName = dto.CustomerName;
        customer.Email = dto.Email;
        customer.Phone = dto.Phone;
        customer.Address = dto.Address;
        customer.IsVeganPreferred = dto.IsVeganPreferred;
        customer.IsActive = dto.IsActive;
        customer.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return customer;
    }

    public async Task<bool> DeactivateCustomerAsync(int customerId)
    {
        var customer = await _context.Customers.FindAsync(customerId);
        if (customer == null)
            return false;

        customer.IsActive = false;
        customer.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<Customer>> SearchCustomersAsync(string searchTerm, int? clientTenantId = null)
    {
        var query = _context.Customers
            .Where(c => c.IsActive && 
                       (c.CustomerName.Contains(searchTerm) || 
                        c.Email.Contains(searchTerm) || 
                        c.Phone.Contains(searchTerm)));

        if (clientTenantId.HasValue)
        {
            query = query.Where(c => c.Orders.Any(o => o.ClientTenantId == clientTenantId));
        }

        return await query
            .OrderBy(c => c.CustomerName)
            .ToListAsync();
    }

    public async Task<CustomerOrderSummaryDto> GetCustomerOrderSummaryAsync(int customerId)
    {
        var customer = await _context.Customers
            .Include(c => c.Orders)
            .FirstOrDefaultAsync(c => c.Id == customerId);

        if (customer == null)
            throw new ArgumentException($"Customer with ID {customerId} not found");

        var completedOrders = customer.Orders.Where(o => o.Status == Domain.Enums.OrderStatus.Completed);

        return new CustomerOrderSummaryDto
        {
            CustomerId = customer.Id,
            CustomerName = customer.CustomerName,
            TotalOrders = customer.Orders.Count,
            TotalSpent = completedOrders.Sum(o => o.TotalAmount),
            LastOrderDate = customer.Orders.OrderByDescending(o => o.OrderDate).FirstOrDefault()?.OrderDate,
            IsVeganPreferred = customer.IsVeganPreferred
        };
    }
}