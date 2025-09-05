using Microsoft.EntityFrameworkCore;
using VHouse.Domain.Entities;
using VHouse.Domain.Enums;
using VHouse.Application.DTOs;
using VHouse.Application.Services;
using VHouse.Infrastructure.Data;

namespace VHouse.Infrastructure.Services;

public class OrderService : IOrderService
{
    private readonly VHouseDbContext _context;

    public OrderService(VHouseDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Order>> GetOrdersByStatusAsync(OrderStatus status, int? clientTenantId = null)
    {
        var query = _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .Where(o => o.Status == status);

        if (clientTenantId.HasValue)
        {
            query = query.Where(o => o.ClientTenantId == clientTenantId);
        }

        return await query
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
    }

    public async Task<Order?> GetOrderByIdAsync(int orderId)
    {
        return await _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == orderId);
    }

    public async Task<Order> CreateOrderAsync(CreateOrderDto dto, int? clientTenantId = null)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        
        try
        {
            var order = new Order
            {
                CustomerId = dto.CustomerId,
                ClientTenantId = clientTenantId,
                Notes = dto.Notes,
                Status = OrderStatus.Pending,
                OrderDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            decimal totalAmount = 0;
            
            foreach (var itemDto in dto.OrderItems)
            {
                var orderItem = new Domain.Entities.OrderItem
                {
                    OrderId = order.Id,
                    ProductId = itemDto.ProductId,
                    Quantity = itemDto.Quantity,
                    UnitPrice = itemDto.UnitPrice,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                totalAmount += itemDto.Quantity * itemDto.UnitPrice;
                _context.OrderItems.Add(orderItem);
            }

            order.TotalAmount = totalAmount;
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return await GetOrderByIdAsync(order.Id) ?? order;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<Order> UpdateOrderStatusAsync(int orderId, OrderStatus status)
    {
        var order = await _context.Orders.FindAsync(orderId);
        if (order == null)
            throw new ArgumentException($"Order with ID {orderId} not found");

        order.Status = status;
        order.UpdatedAt = DateTime.UtcNow;

        if (status == OrderStatus.Completed)
        {
            order.CompletedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return await GetOrderByIdAsync(orderId) ?? order;
    }

    public async Task<Order> AddOrderItemAsync(int orderId, AddOrderItemDto dto)
    {
        var order = await GetOrderByIdAsync(orderId);
        if (order == null)
            throw new ArgumentException($"Order with ID {orderId} not found");

        var orderItem = new Domain.Entities.OrderItem
        {
            OrderId = orderId,
            ProductId = dto.ProductId,
            Quantity = dto.Quantity,
            UnitPrice = dto.UnitPrice,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.OrderItems.Add(orderItem);
        
        order.TotalAmount += dto.Quantity * dto.UnitPrice;
        order.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return await GetOrderByIdAsync(orderId) ?? order;
    }

    public async Task<Order> RemoveOrderItemAsync(int orderId, int orderItemId)
    {
        var orderItem = await _context.OrderItems.FindAsync(orderItemId);
        if (orderItem == null || orderItem.OrderId != orderId)
            throw new ArgumentException($"OrderItem with ID {orderItemId} not found in order {orderId}");

        var order = await GetOrderByIdAsync(orderId);
        if (order == null)
            throw new ArgumentException($"Order with ID {orderId} not found");

        order.TotalAmount -= orderItem.Quantity * orderItem.UnitPrice;
        order.UpdatedAt = DateTime.UtcNow;

        _context.OrderItems.Remove(orderItem);
        await _context.SaveChangesAsync();

        return await GetOrderByIdAsync(orderId) ?? order;
    }

    public async Task<IEnumerable<Order>> GetCustomerOrdersAsync(int customerId)
    {
        return await _context.Orders
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .Where(o => o.CustomerId == customerId)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
    }

    public async Task<decimal> CalculateOrderTotalAsync(int orderId)
    {
        var orderItems = await _context.OrderItems
            .Where(oi => oi.OrderId == orderId)
            .ToListAsync();

        return orderItems.Sum(oi => oi.Quantity * oi.UnitPrice);
    }

    public async Task<OrderSummaryDto> GetOrderSummaryAsync(int? clientTenantId = null, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _context.Orders.AsQueryable();

        if (clientTenantId.HasValue)
        {
            query = query.Where(o => o.ClientTenantId == clientTenantId);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(o => o.OrderDate >= fromDate);
        }

        if (toDate.HasValue)
        {
            query = query.Where(o => o.OrderDate <= toDate);
        }

        var orders = await query.Include(o => o.Customer).ToListAsync();
        var completedOrders = orders.Where(o => o.Status == OrderStatus.Completed);

        return new OrderSummaryDto
        {
            TotalOrders = orders.Count,
            TotalRevenue = completedOrders.Sum(o => o.TotalAmount),
            PendingOrders = orders.Count(o => o.Status == OrderStatus.Pending),
            CompletedOrders = completedOrders.Count(),
            CancelledOrders = orders.Count(o => o.Status == OrderStatus.Cancelled),
            AverageOrderValue = completedOrders.Any() ? completedOrders.Average(o => o.TotalAmount) : 0,
            LastOrderDate = orders.OrderByDescending(o => o.OrderDate).FirstOrDefault()?.OrderDate,
            VeganCustomersCount = orders
                .Where(o => o.Customer.IsVeganPreferred)
                .Select(o => o.CustomerId)
                .Distinct()
                .Count()
        };
    }

    public async Task<bool> CompleteOrderAsync(int orderId)
    {
        var order = await _context.Orders.FindAsync(orderId);
        if (order == null)
            return false;

        order.Status = OrderStatus.Completed;
        order.CompletedAt = DateTime.UtcNow;
        order.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }
}