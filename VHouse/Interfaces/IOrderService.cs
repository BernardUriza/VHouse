using VHouse.Classes;

namespace VHouse.Interfaces
{
    public interface IOrderService
    {
        Task<List<Order>> GetOrdersAsync();
        Task DeleteOrderAsync(int orderId);
        Task<bool> ProcessOrderAsync(Order order);
    }
}