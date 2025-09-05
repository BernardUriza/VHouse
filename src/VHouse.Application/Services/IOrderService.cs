using VHouse.Domain.Entities;
using VHouse.Domain.Enums;
using VHouse.Application.DTOs;

namespace VHouse.Application.Services;

public interface IOrderService
{
    Task<IEnumerable<Order>> GetOrdersByStatusAsync(OrderStatus status, int? clientTenantId = null);
    Task<Order?> GetOrderByIdAsync(int orderId);
    Task<Order> CreateOrderAsync(CreateOrderDto dto, int? clientTenantId = null);
    Task<Order> UpdateOrderStatusAsync(int orderId, OrderStatus status);
    Task<Order> AddOrderItemAsync(int orderId, AddOrderItemDto dto);
    Task<Order> RemoveOrderItemAsync(int orderId, int orderItemId);
    Task<IEnumerable<Order>> GetCustomerOrdersAsync(int customerId);
    Task<decimal> CalculateOrderTotalAsync(int orderId);
    Task<OrderSummaryDto> GetOrderSummaryAsync(int? clientTenantId = null, DateTime? fromDate = null, DateTime? toDate = null);
    Task<bool> CompleteOrderAsync(int orderId);
}