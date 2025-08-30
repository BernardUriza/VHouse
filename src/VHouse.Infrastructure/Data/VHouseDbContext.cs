using Microsoft.EntityFrameworkCore;
using VHouse.Domain.Entities;

namespace VHouse.Infrastructure.Data;

public class VHouseDbContext : DbContext
{
    public VHouseDbContext(DbContextOptions<VHouseDbContext> options) : base(options)
    {
    }

    // Core Clean Architecture entities
    public DbSet<Product> Products { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<Customer> Customers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Product configuration
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.ProductName).IsRequired().HasMaxLength(200);
            entity.Property(p => p.PriceCost).HasColumnType("decimal(18,2)");
            entity.Property(p => p.PriceRetail).HasColumnType("decimal(18,2)");
            entity.Property(p => p.PriceSuggested).HasColumnType("decimal(18,2)");
            entity.Property(p => p.PricePublic).HasColumnType("decimal(18,2)");
        });

        // Customer configuration
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.CustomerName).IsRequired().HasMaxLength(200);
            entity.Property(c => c.Email).HasMaxLength(200);
            entity.HasIndex(c => c.Email).IsUnique();
        });

        // Order configuration
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(o => o.Id);
            entity.Property(o => o.TotalAmount).HasColumnType("decimal(18,2)");
            
            entity.HasOne(o => o.Customer)
                .WithMany(c => c.Orders)
                .HasForeignKey(o => o.CustomerId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // OrderItem configuration
        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(oi => oi.Id);
            entity.Property(oi => oi.UnitPrice).HasColumnType("decimal(18,2)");
            // TotalPrice is a calculated property marked with [NotMapped], so we don't configure it here
            
            entity.HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId);
                
            entity.HasOne(oi => oi.Product)
                .WithMany(p => p.OrderItems)
                .HasForeignKey(oi => oi.ProductId);
        });
    }
}