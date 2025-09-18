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
    public DbSet<Supplier> Suppliers { get; set; }
    
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
    
    // Enterprise audit and monitoring entities
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<SystemMetric> SystemMetrics { get; set; }
    public DbSet<BusinessAlert> BusinessAlerts { get; set; }

    // Gallery entities
    public DbSet<Album> Albums { get; set; }
    public DbSet<Photo> Photos { get; set; }

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
            
            entity.HasOne(p => p.Supplier)
                .WithMany(s => s.Products)
                .HasForeignKey(p => p.SupplierId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Supplier configuration
        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.Property(s => s.Name).IsRequired().HasMaxLength(100);
            entity.Property(s => s.Email).HasMaxLength(100);
            entity.Property(s => s.MinimumOrderAmount).HasColumnType("decimal(18,2)");
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

        // AuditLog configuration
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.Property(a => a.Action).IsRequired().HasMaxLength(50);
            entity.Property(a => a.EntityType).IsRequired().HasMaxLength(100);
            entity.Property(a => a.UserId).IsRequired().HasMaxLength(100);
            entity.Property(a => a.UserName).IsRequired().HasMaxLength(200);
            entity.Property(a => a.IPAddress).HasMaxLength(50);
            entity.Property(a => a.UserAgent).HasMaxLength(200);
            entity.Property(a => a.Changes).HasMaxLength(500);
            entity.Property(a => a.Severity).HasMaxLength(20);
            entity.Property(a => a.Module).HasMaxLength(50);
            entity.Property(a => a.AmountInvolved).HasColumnType("decimal(18,2)");
            entity.Property(a => a.ClientTenant).HasMaxLength(100);
            entity.Property(a => a.ErrorMessage).HasMaxLength(500);
            
            entity.HasIndex(a => a.Timestamp);
            entity.HasIndex(a => a.EntityType);
            entity.HasIndex(a => a.UserId);
            entity.HasIndex(a => a.Severity);
            entity.HasIndex(a => a.Module);
        });

        // SystemMetric configuration
        modelBuilder.Entity<SystemMetric>(entity =>
        {
            entity.HasKey(m => m.Id);
            entity.Property(m => m.MetricName).IsRequired().HasMaxLength(100);
            entity.Property(m => m.MetricType).IsRequired().HasMaxLength(50);
            entity.Property(m => m.Value).HasColumnType("decimal(18,4)");
            entity.Property(m => m.Unit).HasMaxLength(20);
            entity.Property(m => m.Source).HasMaxLength(100);
            entity.Property(m => m.ClientTenant).HasMaxLength(100);
            entity.Property(m => m.Severity).HasMaxLength(20);
            entity.Property(m => m.ActionRequired).HasMaxLength(500);
            
            entity.HasIndex(m => m.Timestamp);
            entity.HasIndex(m => m.MetricType);
            entity.HasIndex(m => m.MetricName);
            entity.HasIndex(m => m.ClientTenant);
            entity.HasIndex(m => m.Severity);
        });

        // BusinessAlert configuration
        modelBuilder.Entity<BusinessAlert>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.Property(a => a.AlertType).IsRequired().HasMaxLength(20);
            entity.Property(a => a.Title).IsRequired().HasMaxLength(200);
            entity.Property(a => a.Description).IsRequired().HasMaxLength(1000);
            entity.Property(a => a.Severity).IsRequired().HasMaxLength(20);
            entity.Property(a => a.ClientTenant).HasMaxLength(100);
            entity.Property(a => a.RelatedEntity).HasMaxLength(100);
            entity.Property(a => a.AmountInvolved).HasColumnType("decimal(18,2)");
            entity.Property(a => a.ResolvedBy).HasMaxLength(200);
            entity.Property(a => a.ResolutionNotes).HasMaxLength(500);
            
            entity.HasIndex(a => a.CreatedAt);
            entity.HasIndex(a => a.Severity);
            entity.HasIndex(a => a.IsResolved);
            entity.HasIndex(a => a.ClientTenant);
            entity.HasIndex(a => a.AlertType);
        });

        // Album configuration
        modelBuilder.Entity<Album>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.Property(a => a.Name).IsRequired().HasMaxLength(100);
            entity.Property(a => a.Slug).IsRequired().HasMaxLength(100);
            entity.HasIndex(a => a.Slug).IsUnique();
        });

        // Photo configuration
        modelBuilder.Entity<Photo>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.FileName).IsRequired().HasMaxLength(500);
            entity.Property(p => p.OriginalName).IsRequired().HasMaxLength(255);
            entity.Property(p => p.ContentType).IsRequired().HasMaxLength(100);
            entity.Property(p => p.Caption).HasMaxLength(500);
            entity.Property(p => p.ThumbnailPath).HasMaxLength(500);

            entity.HasOne(p => p.Album)
                .WithMany(a => a.Photos)
                .HasForeignKey(p => p.AlbumId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(p => p.AlbumId);
            entity.HasIndex(p => p.UploadedUtc);
        });

        SeedGalleryData(modelBuilder);
    }

    private void SeedGalleryData(ModelBuilder modelBuilder)
    {
        var albums = new[]
        {
            new Album { Id = 1, Name = "Products", Slug = "products", Description = "Product catalog photos", CreatedAt = DateTime.UtcNow },
            new Album { Id = 2, Name = "Sales Receipts", Slug = "sales-receipts", Description = "Customer sales receipts", CreatedAt = DateTime.UtcNow },
            new Album { Id = 3, Name = "Purchase Receipts", Slug = "purchase-receipts", Description = "Supplier purchase receipts", CreatedAt = DateTime.UtcNow },
            new Album { Id = 4, Name = "Invoices", Slug = "invoices", Description = "Client invoices and documentation", CreatedAt = DateTime.UtcNow },
            new Album { Id = 5, Name = "Suppliers", Slug = "suppliers", Description = "Supplier documentation and photos", CreatedAt = DateTime.UtcNow },
            new Album { Id = 6, Name = "Customers", Slug = "customers", Description = "Customer documentation and photos", CreatedAt = DateTime.UtcNow },
            new Album { Id = 7, Name = "Misc", Slug = "misc", Description = "Miscellaneous photos and documents", CreatedAt = DateTime.UtcNow }
        };

        modelBuilder.Entity<Album>().HasData(albums);
    }
}