using Microsoft.EntityFrameworkCore;
using VHouse;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Product> Products { get; set; } // Tabla de productos
}
