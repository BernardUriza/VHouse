using VHouse.Classes;

namespace VHouse.Services
{
    /// <summary>
    /// Service for managing orders and saving them in the database.
    /// </summary>
    public class OrderService
    {
        private readonly ApplicationDbContext _context;

        public OrderService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves all orders from the database.
        /// </summary>
        public async Task<List<Order>> GetOrdersAsync()
        {
            return await _context.Orders.Include(o => o.Items).ToListAsync();
        }

        /// <summary>
        /// Saves a new order to the database.
        /// </summary>
        public async Task SaveOrderAsync(Order order)
        {
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
        }
    }
}
