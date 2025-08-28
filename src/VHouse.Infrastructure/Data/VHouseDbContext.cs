using Microsoft.EntityFrameworkCore;
using VHouse.Domain.Entities;

namespace VHouse.Infrastructure.Data;

public class VHouseDbContext : DbContext
{
    public VHouseDbContext(DbContextOptions<VHouseDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Product configuration
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ProductName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Emoji).HasMaxLength(10);
            entity.Property(e => e.PriceCost).HasColumnType("decimal(18,2)");
            entity.Property(e => e.PriceRetail).HasColumnType("decimal(18,2)");
            entity.Property(e => e.PriceSuggested).HasColumnType("decimal(18,2)");
            entity.Property(e => e.PricePublic).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Description).HasMaxLength(500);
        });

        // Customer configuration
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CustomerName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Email).HasMaxLength(200);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.Address).HasMaxLength(500);
        });

        // Order configuration
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Notes).HasMaxLength(1000);
            
            entity.HasOne(e => e.Customer)
                .WithMany(c => c.Orders)
                .HasForeignKey(e => e.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // OrderItem configuration
        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)");
            
            entity.HasOne(e => e.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(e => e.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.Product)
                .WithMany(p => p.OrderItems)
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Seed data
        modelBuilder.Entity<Product>().HasData(
            new Product 
            { 
                Id = 1, 
                ProductName = "Tocino Vegano", 
                Emoji = "🥓", 
                PriceCost = 42.00m, 
                PriceRetail = 48.00m, 
                PriceSuggested = 70.00m, 
                PricePublic = 62.00m,
                Description = "Tocino 100% vegetal",
                StockQuantity = 50,
                IsVegan = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Product 
            { 
                Id = 2, 
                ProductName = "Proteína Texturizada", 
                Emoji = "🥩", 
                PriceCost = 65.00m, 
                PriceRetail = 70.00m, 
                PriceSuggested = 90.00m, 
                PricePublic = 69.00m,
                Description = "Proteína de soya texturizada",
                StockQuantity = 30,
                IsVegan = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        );
    }
}