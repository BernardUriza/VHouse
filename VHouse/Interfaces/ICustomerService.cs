using VHouse.Classes;

namespace VHouse.Interfaces
{
    public interface ICustomerService
    {
        Task<List<Customer>> GetCustomersAsync();
        Task<Inventory> GetInventoryAsync(int customerId);
        Task AddCustomerAsync(Customer customer);
        Task UpdateCustomerAsync(Customer customer);
        Task DeleteCustomerAsync(int customerId);
    }
}