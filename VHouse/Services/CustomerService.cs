using Microsoft.EntityFrameworkCore;
using VHouse.Classes;

namespace VHouse.Services
{
    /// <summary>
    /// Service for managing customers.
    /// </summary>
    public class CustomerService
    {
        private readonly ApplicationDbContext _context;

        public CustomerService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves all customers from the database.
        /// </summary>
        public async Task<List<Customer>> GetCustomersAsync()
        {
            return await _context.Customers.Include(c => c.Orders).Include(u => u.Inventory).ThenInclude(u => u.Items).ThenInclude(u => u.Product).ToListAsync();
        }

        /// <summary>
        /// Retrieves the inventory of a specific customer.
        /// </summary>
        public async Task<Inventory> GetInventoryAsync(int customerId)
        {
            var inventory = await _context.Inventories
                .Where(i => i.CustomerId == customerId)
                .Include(i => i.Items)
                .FirstOrDefaultAsync();
            if(inventory == null)
                inventory  = new() { Items = new List<InventoryItem>() };

            return inventory;
        }

        /// <summary>
        /// Adds a new customer to the database.
        /// </summary>
        public async Task AddCustomerAsync(Customer customer)
        {
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Updates an existing customer's information.
        /// </summary>
        public async Task UpdateCustomerAsync(Customer customer)
        {
            _context.Customers.Update(customer);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Deletes a customer from the database.
        /// </summary>
        public async Task DeleteCustomerAsync(int customerId)
        {
            var customer = await _context.Customers.FindAsync(customerId);
            if (customer != null)
            {
                _context.Customers.Remove(customer);
                await _context.SaveChangesAsync();
            }
        }
    }
}
