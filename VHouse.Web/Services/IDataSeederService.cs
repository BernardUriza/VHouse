// Creado por Bernard Orozco
using Microsoft.EntityFrameworkCore;
using VHouse.Infrastructure.Data;

namespace VHouse.Web.Services;

public interface IDataSeederService
{
    Task SeedAsync();
}

public class DataSeederService : IDataSeederService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<DataSeederService> _logger;

    public DataSeederService(IServiceScopeFactory serviceScopeFactory, ILogger<DataSeederService> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var services = scope.ServiceProvider;
        var context = services.GetRequiredService<VHouseDbContext>();
        
        _logger.LogInformation("📦 Applying migrations...");
        await context.Database.MigrateAsync();
        _logger.LogInformation("✅ Migrations applied successfully.");

        _logger.LogInformation("📦 Applying seeds...");
        await SeedBasicProducts(context);
        var passwordService = services.GetRequiredService<VHouse.Domain.Interfaces.IPasswordService>();
        await VHouse.Web.Data.DbSeeder.SeedMonaLaDonaAsync(context, passwordService);
        _logger.LogInformation("✅ Seeds applied successfully.");
    }

    private async Task SeedBasicProducts(VHouseDbContext context)
    {
        if (!context.Products.Any())
        {
            var sampleProducts = new[]
            {
                new VHouse.Domain.Entities.Product
                {
                    ProductName = "Queso Vegano Artesanal",
                    Emoji = "🧀",
                    PriceCost = 80.00m,
                    PriceRetail = 120.00m,
                    PriceSuggested = 140.00m,
                    PricePublic = 150.00m,
                    Description = "Delicioso queso vegano hecho con nueces de macadamia",
                    StockQuantity = 25,
                    IsVegan = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new VHouse.Domain.Entities.Product
                {
                    ProductName = "Hamburguesa Plant-Based",
                    Emoji = "🍔",
                    PriceCost = 45.00m,
                    PriceRetail = 75.00m,
                    PriceSuggested = 85.00m,
                    PricePublic = 95.00m,
                    Description = "Hamburguesa 100% vegetal con proteína de soya",
                    StockQuantity = 50,
                    IsVegan = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new VHouse.Domain.Entities.Product
                {
                    ProductName = "Leche de Almendra Orgánica",
                    Emoji = "🥛",
                    PriceCost = 25.00m,
                    PriceRetail = 45.00m,
                    PriceSuggested = 50.00m,
                    PricePublic = 55.00m,
                    Description = "Leche de almendra orgánica sin azúcar añadida",
                    StockQuantity = 30,
                    IsVegan = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new VHouse.Domain.Entities.Product
                {
                    ProductName = "Pizza Vegana Margarita",
                    Emoji = "🍕",
                    PriceCost = 60.00m,
                    PriceRetail = 95.00m,
                    PriceSuggested = 110.00m,
                    PricePublic = 120.00m,
                    Description = "Pizza con queso vegano y albahaca fresca",
                    StockQuantity = 15,
                    IsVegan = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new VHouse.Domain.Entities.Product
                {
                    ProductName = "Yogurt de Coco Natural",
                    Emoji = "🥥",
                    PriceCost = 20.00m,
                    PriceRetail = 35.00m,
                    PriceSuggested = 40.00m,
                    PricePublic = 45.00m,
                    Description = "Yogurt cremoso de coco natural probiótico",
                    StockQuantity = 40,
                    IsVegan = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }
            };

            context.Products.AddRange(sampleProducts);
            await context.SaveChangesAsync();
            _logger.LogInformation("✅ Added {ProductCount} sample products to database", sampleProducts.Length);
        }
        else
        {
            _logger.LogInformation("ℹ️ Sample products already exist in database");
        }
    }
}

public static class DataSeederServiceExtensions
{
    public static IServiceCollection AddDataSeeder(this IServiceCollection services)
    {
        services.AddScoped<IDataSeederService, DataSeederService>();
        return services;
    }

    public static async Task<WebApplication> SeedDataAsync(this WebApplication app, IConfiguration configuration)
    {
        var skipMigrations = configuration.GetValue<bool>("SkipMigrations");
        if (!skipMigrations)
        {
            using var scope = app.Services.CreateScope();
            var seeder = scope.ServiceProvider.GetRequiredService<IDataSeederService>();
            await seeder.SeedAsync();
        }
        
        return app;
    }
}