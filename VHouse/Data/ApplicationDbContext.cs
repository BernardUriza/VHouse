using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VHouse;
using VHouse.Classes;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Product> Products { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Inventory> Inventories { get; set; }
    public DbSet<InventoryItem> InventoryItems { get; set; }
    public DbSet<Invoice> Invoices { get; set; }
    
    // B2B entities
    public DbSet<Supplier> Suppliers { get; set; }
    public DbSet<Brand> Brands { get; set; }
    public DbSet<PurchaseOrder> PurchaseOrders { get; set; }
    public DbSet<PurchaseOrderItem> PurchaseOrderItems { get; set; }
    public DbSet<Warehouse> Warehouses { get; set; }
    public DbSet<WarehouseInventory> WarehouseInventories { get; set; }
    public DbSet<ShrinkageRecord> ShrinkageRecords { get; set; }
    // dotnet ef migrations add AddNullableInvoiceIdAndGeneralInventorySupport
    // dotnet ef database update

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configurar relaciones
        modelBuilder.Entity<Customer>()
            .HasOne(i => i.Inventory)
            .WithOne(ii => ii.Customer)
            .HasForeignKey<Inventory>(i => i.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Inventory>()
            .HasMany(i => i.Items)
            .WithOne(ii => ii.Inventory)
            .HasForeignKey(ii => ii.InventoryId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<InventoryItem>()
            .HasOne(i => i.Invoice)
            .WithMany() 
            .HasForeignKey(i => i.InvoiceId)
            .OnDelete(DeleteBehavior.SetNull);

        // Configure ApplicationUser relationship with Customer
        modelBuilder.Entity<ApplicationUser>()
            .HasOne(u => u.Customer)
            .WithMany()
            .HasForeignKey(u => u.CustomerId)
            .OnDelete(DeleteBehavior.SetNull);

        // B2B entity relationships
        
        // Product-Brand relationship
        modelBuilder.Entity<Product>()
            .HasOne(p => p.Brand)
            .WithMany(b => b.Products)
            .HasForeignKey(p => p.BrandId)
            .OnDelete(DeleteBehavior.SetNull);

        // Product-Supplier relationship
        modelBuilder.Entity<Product>()
            .HasOne(p => p.Supplier)
            .WithMany(s => s.Products)
            .HasForeignKey(p => p.SupplierId)
            .OnDelete(DeleteBehavior.SetNull);

        // PurchaseOrder-Supplier relationship
        modelBuilder.Entity<PurchaseOrder>()
            .HasOne(po => po.Supplier)
            .WithMany(s => s.PurchaseOrders)
            .HasForeignKey(po => po.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);

        // PurchaseOrder-Warehouse relationship
        modelBuilder.Entity<PurchaseOrder>()
            .HasOne(po => po.Warehouse)
            .WithMany(w => w.PurchaseOrders)
            .HasForeignKey(po => po.WarehouseId)
            .OnDelete(DeleteBehavior.SetNull);

        // PurchaseOrderItem relationships
        modelBuilder.Entity<PurchaseOrderItem>()
            .HasOne(poi => poi.PurchaseOrder)
            .WithMany(po => po.Items)
            .HasForeignKey(poi => poi.PurchaseOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<PurchaseOrderItem>()
            .HasOne(poi => poi.Product)
            .WithMany(p => p.PurchaseOrderItems)
            .HasForeignKey(poi => poi.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        // WarehouseInventory relationships
        modelBuilder.Entity<WarehouseInventory>()
            .HasOne(wi => wi.Warehouse)
            .WithMany(w => w.InventoryItems)
            .HasForeignKey(wi => wi.WarehouseId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<WarehouseInventory>()
            .HasOne(wi => wi.Product)
            .WithMany(p => p.WarehouseInventories)
            .HasForeignKey(wi => wi.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        // ShrinkageRecord relationships
        modelBuilder.Entity<ShrinkageRecord>()
            .HasOne(sr => sr.Product)
            .WithMany(p => p.ShrinkageRecords)
            .HasForeignKey(sr => sr.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ShrinkageRecord>()
            .HasOne(sr => sr.Warehouse)
            .WithMany()
            .HasForeignKey(sr => sr.WarehouseId)
            .OnDelete(DeleteBehavior.SetNull);

        // Unique constraints
        modelBuilder.Entity<WarehouseInventory>()
            .HasIndex(wi => new { wi.WarehouseId, wi.ProductId })
            .IsUnique();

        modelBuilder.Entity<Warehouse>()
            .HasIndex(w => w.Code)
            .IsUnique();
    }
}
