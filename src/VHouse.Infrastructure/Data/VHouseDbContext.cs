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
    
    // B2B Client Portal entities
    public DbSet<ClientTenant> ClientTenants { get; set; }
    public DbSet<ClientProduct> ClientProducts { get; set; }
    
    // Delivery tracking entities
    public DbSet<Delivery> Deliveries { get; set; }
    public DbSet<DeliveryItem> DeliveryItems { get; set; }
    
    // Consignment system entities  
    public DbSet<Consignment> Consignments { get; set; }
    public DbSet<ConsignmentItem> ConsignmentItems { get; set; }
    public DbSet<ConsignmentSale> ConsignmentSales { get; set; }

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

        // ClientTenant configuration
        modelBuilder.Entity<ClientTenant>(entity =>
        {
            entity.HasKey(ct => ct.Id);
            entity.Property(ct => ct.TenantCode).IsRequired().HasMaxLength(50);
            entity.Property(ct => ct.LoginUsername).IsRequired().HasMaxLength(50);
            entity.HasIndex(ct => ct.TenantCode).IsUnique();
            entity.HasIndex(ct => ct.LoginUsername).IsUnique();
        });

        // ClientProduct configuration
        modelBuilder.Entity<ClientProduct>(entity =>
        {
            entity.HasKey(cp => cp.Id);
            entity.Property(cp => cp.CustomPrice).HasColumnType("decimal(18,2)");
            
            entity.HasOne(cp => cp.ClientTenant)
                .WithMany(ct => ct.ClientProducts)
                .HasForeignKey(cp => cp.ClientTenantId);
                
            entity.HasOne(cp => cp.Product)
                .WithMany()
                .HasForeignKey(cp => cp.ProductId);
                
            // Prevent duplicate assignments
            entity.HasIndex(cp => new { cp.ClientTenantId, cp.ProductId }).IsUnique();
        });

        // Delivery configuration
        modelBuilder.Entity<Delivery>(entity =>
        {
            entity.HasKey(d => d.Id);
            entity.Property(d => d.DeliveryNumber).IsRequired().HasMaxLength(50);
            entity.Property(d => d.TotalAmount).HasColumnType("decimal(18,2)");
            entity.Property(d => d.DeliveryLatitude).HasColumnType("decimal(10,8)");
            entity.Property(d => d.DeliveryLongitude).HasColumnType("decimal(11,8)");
            
            entity.HasOne(d => d.Order)
                .WithMany()
                .HasForeignKey(d => d.OrderId);
                
            entity.HasOne(d => d.ClientTenant)
                .WithMany()
                .HasForeignKey(d => d.ClientTenantId);
                
            entity.HasIndex(d => d.DeliveryNumber).IsUnique();
            entity.HasIndex(d => d.DeliveryDate);
            entity.HasIndex(d => d.Status);
        });

        // DeliveryItem configuration
        modelBuilder.Entity<DeliveryItem>(entity =>
        {
            entity.HasKey(di => di.Id);
            entity.Property(di => di.UnitPrice).HasColumnType("decimal(18,2)");
            entity.Property(di => di.TotalPrice).HasColumnType("decimal(18,2)");
            
            entity.HasOne(di => di.Delivery)
                .WithMany(d => d.DeliveryItems)
                .HasForeignKey(di => di.DeliveryId);
                
            entity.HasOne(di => di.Product)
                .WithMany()
                .HasForeignKey(di => di.ProductId);
        });

        // Consignment configuration
        modelBuilder.Entity<Consignment>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.ConsignmentNumber).IsRequired().HasMaxLength(50);
            entity.Property(c => c.TotalValueAtCost).HasColumnType("decimal(18,2)");
            entity.Property(c => c.TotalValueAtRetail).HasColumnType("decimal(18,2)");
            entity.Property(c => c.StorePercentage).HasColumnType("decimal(5,2)");
            entity.Property(c => c.BernardPercentage).HasColumnType("decimal(5,2)");
            entity.Property(c => c.TotalSold).HasColumnType("decimal(18,2)");
            entity.Property(c => c.AmountDueToBernard).HasColumnType("decimal(18,2)");
            entity.Property(c => c.AmountDueToStore).HasColumnType("decimal(18,2)");
            
            entity.HasOne(c => c.ClientTenant)
                .WithMany()
                .HasForeignKey(c => c.ClientTenantId);
                
            entity.HasIndex(c => c.ConsignmentNumber).IsUnique();
            entity.HasIndex(c => c.ConsignmentDate);
            entity.HasIndex(c => c.Status);
        });

        // ConsignmentItem configuration
        modelBuilder.Entity<ConsignmentItem>(entity =>
        {
            entity.HasKey(ci => ci.Id);
            entity.Property(ci => ci.CostPrice).HasColumnType("decimal(18,2)");
            entity.Property(ci => ci.RetailPrice).HasColumnType("decimal(18,2)");
            
            entity.HasOne(ci => ci.Consignment)
                .WithMany(c => c.ConsignmentItems)
                .HasForeignKey(ci => ci.ConsignmentId);
                
            entity.HasOne(ci => ci.Product)
                .WithMany()
                .HasForeignKey(ci => ci.ProductId);
        });

        // ConsignmentSale configuration
        modelBuilder.Entity<ConsignmentSale>(entity =>
        {
            entity.HasKey(cs => cs.Id);
            entity.Property(cs => cs.UnitPrice).HasColumnType("decimal(18,2)");
            entity.Property(cs => cs.TotalSaleAmount).HasColumnType("decimal(18,2)");
            entity.Property(cs => cs.StoreAmount).HasColumnType("decimal(18,2)");
            entity.Property(cs => cs.BernardAmount).HasColumnType("decimal(18,2)");
            
            entity.HasOne(cs => cs.Consignment)
                .WithMany(c => c.ConsignmentSales)
                .HasForeignKey(cs => cs.ConsignmentId);
                
            entity.HasOne(cs => cs.ConsignmentItem)
                .WithMany()
                .HasForeignKey(cs => cs.ConsignmentItemId);
                
            entity.HasIndex(cs => cs.SaleDate);
        });
    }
}