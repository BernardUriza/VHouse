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
    
    // Phase 5: Advanced Distribution entities
    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<DistributionCenter> DistributionCenters { get; set; }
    public DbSet<DeliveryRoute> DeliveryRoutes { get; set; }
    public DbSet<Delivery> Deliveries { get; set; }
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

        // Performance indexes
        
        // Product indexes for fast lookups
        modelBuilder.Entity<Product>()
            .HasIndex(p => p.SKU)
            .HasDatabaseName("IX_Product_SKU");
            
        modelBuilder.Entity<Product>()
            .HasIndex(p => p.Barcode)
            .HasDatabaseName("IX_Product_Barcode");
            
        modelBuilder.Entity<Product>()
            .HasIndex(p => p.IsActive)
            .HasDatabaseName("IX_Product_IsActive");
            
        modelBuilder.Entity<Product>()
            .HasIndex(p => new { p.BrandId, p.IsActive })
            .HasDatabaseName("IX_Product_Brand_Active");

        // Customer indexes
        modelBuilder.Entity<Customer>()
            .HasIndex(c => c.Email)
            .HasDatabaseName("IX_Customer_Email");
            
        modelBuilder.Entity<Customer>()
            .HasIndex(c => c.IsRetail)
            .HasDatabaseName("IX_Customer_IsRetail");

        // Order indexes for reporting and analytics
        modelBuilder.Entity<Order>()
            .HasIndex(o => o.OrderDate)
            .HasDatabaseName("IX_Order_Date");
            
        modelBuilder.Entity<Order>()
            .HasIndex(o => new { o.CustomerId, o.OrderDate })
            .HasDatabaseName("IX_Order_Customer_Date");

        // Supplier indexes
        modelBuilder.Entity<Supplier>()
            .HasIndex(s => s.IsActive)
            .HasDatabaseName("IX_Supplier_IsActive");
            
        modelBuilder.Entity<Supplier>()
            .HasIndex(s => s.Email)
            .HasDatabaseName("IX_Supplier_Email");

        // PurchaseOrder indexes
        modelBuilder.Entity<PurchaseOrder>()
            .HasIndex(po => po.OrderDate)
            .HasDatabaseName("IX_PurchaseOrder_Date");
            
        modelBuilder.Entity<PurchaseOrder>()
            .HasIndex(po => po.Status)
            .HasDatabaseName("IX_PurchaseOrder_Status");
            
        modelBuilder.Entity<PurchaseOrder>()
            .HasIndex(po => new { po.SupplierId, po.Status })
            .HasDatabaseName("IX_PurchaseOrder_Supplier_Status");

        // Warehouse inventory indexes
        modelBuilder.Entity<WarehouseInventory>()
            .HasIndex(wi => wi.LastUpdated)
            .HasDatabaseName("IX_WarehouseInventory_LastUpdated");
            
        modelBuilder.Entity<WarehouseInventory>()
            .HasIndex(wi => wi.QuantityOnHand)
            .HasDatabaseName("IX_WarehouseInventory_Quantity");

        // Shrinkage indexes for reporting
        modelBuilder.Entity<ShrinkageRecord>()
            .HasIndex(sr => sr.DiscoveryDate)
            .HasDatabaseName("IX_ShrinkageRecord_DiscoveryDate");
            
        modelBuilder.Entity<ShrinkageRecord>()
            .HasIndex(sr => sr.Reason)
            .HasDatabaseName("IX_ShrinkageRecord_Reason");
            
        modelBuilder.Entity<ShrinkageRecord>()
            .HasIndex(sr => sr.IsApproved)
            .HasDatabaseName("IX_ShrinkageRecord_IsApproved");

        // Brand indexes
        modelBuilder.Entity<Brand>()
            .HasIndex(b => b.IsActive)
            .HasDatabaseName("IX_Brand_IsActive");

        // Inventory indexes
        modelBuilder.Entity<Inventory>()
            .HasIndex(i => i.CustomerId)
            .HasDatabaseName("IX_Inventory_Customer");
            
        modelBuilder.Entity<InventoryItem>()
            .HasIndex(ii => ii.ExpirationDate)
            .HasDatabaseName("IX_InventoryItem_Expiration");

        // Phase 5: Tenant and Distribution Center relationships
        
        // Tenant-DistributionCenter relationship
        modelBuilder.Entity<DistributionCenter>()
            .HasOne(dc => dc.Tenant)
            .WithMany(t => t.DistributionCenters)
            .HasForeignKey(dc => dc.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        // Tenant-DeliveryRoute relationship
        modelBuilder.Entity<DeliveryRoute>()
            .HasOne(dr => dr.Tenant)
            .WithMany(t => t.DeliveryRoutes)
            .HasForeignKey(dr => dr.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        // DistributionCenter-DeliveryRoute relationship
        modelBuilder.Entity<DeliveryRoute>()
            .HasOne(dr => dr.DistributionCenter)
            .WithMany(dc => dc.DeliveryRoutes)
            .HasForeignKey(dr => dr.DistributionCenterId)
            .OnDelete(DeleteBehavior.Cascade);

        // DistributionCenter-Warehouse relationship
        modelBuilder.Entity<Warehouse>()
            .HasOne(w => w.DistributionCenter)
            .WithMany(dc => dc.Warehouses)
            .HasForeignKey(w => w.DistributionCenterId)
            .OnDelete(DeleteBehavior.SetNull);

        // DeliveryRoute-Delivery relationship
        modelBuilder.Entity<Delivery>()
            .HasOne(d => d.DeliveryRoute)
            .WithMany(dr => dr.Deliveries)
            .HasForeignKey(d => d.DeliveryRouteId)
            .OnDelete(DeleteBehavior.SetNull);

        // Delivery-Order relationship
        modelBuilder.Entity<Delivery>()
            .HasOne(d => d.Order)
            .WithMany()
            .HasForeignKey(d => d.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // Unique constraints for Phase 5
        modelBuilder.Entity<Tenant>()
            .HasIndex(t => t.TenantCode)
            .IsUnique();

        modelBuilder.Entity<DistributionCenter>()
            .HasIndex(dc => new { dc.TenantId, dc.CenterCode })
            .IsUnique();

        modelBuilder.Entity<DeliveryRoute>()
            .HasIndex(dr => new { dr.TenantId, dr.RouteCode })
            .IsUnique();

        // Performance indexes for Phase 5
        
        // Tenant indexes
        modelBuilder.Entity<Tenant>()
            .HasIndex(t => t.IsActive)
            .HasDatabaseName("IX_Tenant_IsActive");

        modelBuilder.Entity<Tenant>()
            .HasIndex(t => t.ContactEmail)
            .HasDatabaseName("IX_Tenant_ContactEmail");

        // DistributionCenter indexes
        modelBuilder.Entity<DistributionCenter>()
            .HasIndex(dc => dc.IsActive)
            .HasDatabaseName("IX_DistributionCenter_IsActive");

        modelBuilder.Entity<DistributionCenter>()
            .HasIndex(dc => new { dc.Latitude, dc.Longitude })
            .HasDatabaseName("IX_DistributionCenter_Location");

        // DeliveryRoute indexes
        modelBuilder.Entity<DeliveryRoute>()
            .HasIndex(dr => dr.IsActive)
            .HasDatabaseName("IX_DeliveryRoute_IsActive");

        modelBuilder.Entity<DeliveryRoute>()
            .HasIndex(dr => new { dr.DistributionCenterId, dr.IsActive })
            .HasDatabaseName("IX_DeliveryRoute_Center_Active");

        // Delivery indexes
        modelBuilder.Entity<Delivery>()
            .HasIndex(d => d.Status)
            .HasDatabaseName("IX_Delivery_Status");

        modelBuilder.Entity<Delivery>()
            .HasIndex(d => d.ScheduledDate)
            .HasDatabaseName("IX_Delivery_ScheduledDate");

        modelBuilder.Entity<Delivery>()
            .HasIndex(d => new { d.DeliveryRouteId, d.Status })
            .HasDatabaseName("IX_Delivery_Route_Status");

        modelBuilder.Entity<Delivery>()
            .HasIndex(d => new { d.Latitude, d.Longitude })
            .HasDatabaseName("IX_Delivery_Location");
    }
}
