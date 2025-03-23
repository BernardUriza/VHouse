using Microsoft.EntityFrameworkCore;
using VHouse;
using VHouse.Classes;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Product> Products { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Inventory> Inventories { get; set; }
    public DbSet<InventoryItem> InventoryItems { get; set; }
    public DbSet<Invoice> Invoices { get; set; }
    // dotnet ef migrations add AddNullableInvoiceIdAndGeneralInventorySupport
    // dotnet ef database update

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configurar relaciones
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

    }
}
