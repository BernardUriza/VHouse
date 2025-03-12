using Microsoft.EntityFrameworkCore;
using VHouse;
using VHouse.Classes;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Product> Products { get; set; } 
    public DbSet<Order> Orders { get; set; }
    public DbSet<Customer> Customers { get; set; }
}
