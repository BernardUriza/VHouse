using VHouse.Domain.Entities;
using VHouse.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using VHouse.Domain.Interfaces;

namespace VHouse.Web.Data;

public static class DbSeeder
{
    public static async Task SeedMonaLaDonaAsync(VHouseDbContext context, IPasswordService passwordService)
    {
        // Verificar si Mona la Dona ya existe
        var existingClient = await context.ClientTenants
            .FirstOrDefaultAsync(ct => ct.TenantCode == "MONA_DONA");
        
        if (existingClient != null)
        {
            Console.WriteLine("🍩 Mona la Dona ya existe en la base de datos.");
            return;
        }
        
        Console.WriteLine("🌱 Creando cliente Mona la Dona y sus productos veganos...");
        
        // Crear cliente Mona la Dona
        var monaLaDona = new ClientTenant
        {
            TenantName = "Mona la Dona",
            TenantCode = "MONA_DONA",
            BusinessName = "Mona la Dona - Donas Veganas Premium",
            Description = "Especialistas en donas veganas artesanales que derriten corazones y no lastiman animales 🍩",
            ContactPerson = "Ana Sofía González",
            Email = "ana@monaladorona.com.mx",
            Phone = "+52 33 1234 5678",
            LoginUsername = "monadona",
            LoginPasswordHash = passwordService.HashPassword(Environment.GetEnvironmentVariable("MONA_DONA_PASSWORD") ?? "VeganaPoderosa2024!"),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        
        context.ClientTenants.Add(monaLaDona);
        await context.SaveChangesAsync();
        
        // Productos específicos para Mona la Dona
        var products = new List<Product>
        {
            new Product
            {
                ProductName = "Leche de Avena Premium 1L",
                Description = "Leche de avena cremosa perfecta para donas esponjosas sin lácteos",
                PriceCost = 25.50m,
                PriceRetail = 45.00m,
                PriceSuggested = 42.00m,
                PricePublic = 38.00m,
                StockQuantity = 50,
                IsVegan = true,
                IsActive = true,
                Emoji = "🥛"
            },
            new Product
            {
                ProductName = "Mantequilla Vegana Natural 250g",
                Description = "Mantequilla 100% vegetal ideal para masa de donas",
                PriceCost = 35.00m,
                PriceRetail = 62.00m,
                PriceSuggested = 58.00m,
                PricePublic = 52.00m,
                StockQuantity = 30,
                IsVegan = true,
                IsActive = true,
                Emoji = "🧈"
            },
            new Product
            {
                ProductName = "Queso Crema Q Foods 200g",
                Description = "Queso crema vegano Q Foods para rellenos cremosos",
                PriceCost = 42.00m,
                PriceRetail = 72.00m,
                PriceSuggested = 68.00m,
                PricePublic = 62.00m,
                StockQuantity = 25,
                IsVegan = true,
                IsActive = true,
                Emoji = "🧀"
            },
            new Product
            {
                ProductName = "Queso Amarillo Q Foods 200g",
                Description = "Queso amarillo vegano Q Foods perfecto para donas saladas",
                PriceCost = 38.00m,
                PriceRetail = 68.00m,
                PriceSuggested = 64.00m,
                PricePublic = 58.00m,
                StockQuantity = 25,
                IsVegan = true,
                IsActive = true,
                Emoji = "🟡"
            },
            new Product
            {
                ProductName = "Salchicha Vegana Ahumada 200g",
                Description = "Salchicha vegana ahumada para donas gourmet saladas",
                PriceCost = 48.00m,
                PriceRetail = 82.00m,
                PriceSuggested = 78.00m,
                PricePublic = 72.00m,
                StockQuantity = 15,
                IsVegan = true,
                IsActive = true,
                Emoji = "🌭"
            },
            new Product
            {
                ProductName = "Jamón Vegano Meathical 150g",
                Description = "Jamón vegano Meathical premium para donas de desayuno",
                PriceCost = 52.00m,
                PriceRetail = 88.00m,
                PriceSuggested = 84.00m,
                PricePublic = 78.00m,
                StockQuantity = 12,
                IsVegan = true,
                IsActive = true,
                Emoji = "🥓"
            },
            new Product
            {
                ProductName = "Cajeta Vegana Sin Azúcar 300ml",
                Description = "Cajeta vegana sin azúcar refinada para glaseados naturales",
                PriceCost = 55.00m,
                PriceRetail = 92.00m,
                PriceSuggested = 88.00m,
                PricePublic = 82.00m,
                StockQuantity = 20,
                IsVegan = true,
                IsActive = true,
                Emoji = "🍮"
            },
            new Product
            {
                ProductName = "Setas Mixtas Premium 250g",
                Description = "Mezcla de setas frescas para donas saladas gourmet",
                PriceCost = 28.00m,
                PriceRetail = 48.00m,
                PriceSuggested = 45.00m,
                PricePublic = 42.00m,
                StockQuantity = 35,
                IsVegan = true,
                IsActive = true,
                Emoji = "🍄"
            },
            new Product
            {
                ProductName = "Kombucha LAffectuosyta 500ml",
                Description = "Kombucha probiótica artesanal perfecta para acompañar donas",
                PriceCost = 45.00m,
                PriceRetail = 75.00m,
                PriceSuggested = 72.00m,
                PricePublic = 68.00m,
                StockQuantity = 40,
                IsVegan = true,
                IsActive = true,
                Emoji = "🫧"
            }
        };
        
        // Insertar productos
        foreach (var product in products)
        {
            var existingProduct = await context.Products
                .FirstOrDefaultAsync(p => p.ProductName == product.ProductName);
                
            if (existingProduct == null)
            {
                context.Products.Add(product);
            }
        }
        
        await context.SaveChangesAsync();
        
        // Asignar productos a Mona la Dona
        var insertedProducts = await context.Products
            .Where(p => products.Select(np => np.ProductName).Contains(p.ProductName))
            .ToListAsync();
            
        var clientProducts = new List<ClientProduct>();
        
        foreach (var product in insertedProducts)
        {
            var minQuantity = product.ProductName switch
            {
                var name when name.Contains("Leche") => 2,
                var name when name.Contains("Mantequilla") => 3,
                var name when name.Contains("Queso") => 2,
                var name when name.Contains("Salchicha") => 1,
                var name when name.Contains("Jamón") => 1,
                var name when name.Contains("Cajeta") => 1,
                var name when name.Contains("Setas") => 2,
                var name when name.Contains("Kombucha") => 3,
                _ => 1
            };
            
            clientProducts.Add(new ClientProduct
            {
                ClientTenantId = monaLaDona.Id,
                ProductId = product.Id,
                CustomPrice = product.PricePublic, // Precio especial B2B
                MinOrderQuantity = minQuantity,
                IsAvailable = true,
                AssignedAt = DateTime.UtcNow
            });
        }
        
        context.ClientProducts.AddRange(clientProducts);
        await context.SaveChangesAsync();
        
        Console.WriteLine($"✅ Mona la Dona creada exitosamente con {clientProducts.Count} productos asignados.");
        Console.WriteLine("🍩 Usuario: monadona | Contraseña: vegandoñas2024#");
        Console.WriteLine("🌱 ¡Listo para hacer donas veganas que salven vidas!");
    }
}