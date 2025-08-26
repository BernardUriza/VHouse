using Microsoft.EntityFrameworkCore;
using VHouse.Classes;

namespace VHouse.Data
{
    public class VHouseContext : DbContext
    {
        public VHouseContext(DbContextOptions<VHouseContext> options) : base(options)
        {
        }

        // Core entities
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Inventory> Inventories { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Brand> Brands { get; set; }
        public DbSet<Warehouse> Warehouses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Add basic configurations here if needed
            modelBuilder.Entity<Product>()
                .HasIndex(p => p.SKU)
                .IsUnique();
                
            modelBuilder.Entity<Customer>()
                .HasIndex(c => c.Email)
                .IsUnique();
        }
    }
}