using VHouse.Domain.Entities;
using VHouse.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using VHouse.Domain.Interfaces;

namespace VHouse.Web.Data;

public static class DbSeeder
{
    public static async Task SeedCoreProductsAsync(VHouseDbContext context)
    {
        // Verificar si ya existen productos base
        var existingProductsCount = await context.Products.CountAsync();

        if (existingProductsCount > 0)
        {
            Console.WriteLine($"🌱 Ya existen {existingProductsCount} productos en la base de datos.");
            return;
        }

        Console.WriteLine("📦 Poblando productos veganos base...");

        // 20 productos veganos reales de la operación de VHouse
        var coreProducts = new List<Product>
        {
            new Product { ProductName = "Gelatina Vegana", Emoji = "🍮", PriceCost = 30.00m, PriceRetail = 35.00m, PriceSuggested = 35.00m, PricePublic = 35.00m, StockQuantity = 50, IsVegan = true, IsActive = true },
            new Product { ProductName = "Tocino 200g", Emoji = "🥓", PriceCost = 42.00m, PriceRetail = 42.00m, PriceSuggested = 42.00m, PricePublic = 42.00m, StockQuantity = 30, IsVegan = true, IsActive = true },
            new Product { ProductName = "Pepperoni 250g", Emoji = "🍕", PriceCost = 42.00m, PriceRetail = 48.00m, PriceSuggested = 48.00m, PricePublic = 48.00m, StockQuantity = 25, IsVegan = true, IsActive = true },
            new Product { ProductName = "Jamón 270g", Emoji = "🍖", PriceCost = 50.00m, PriceRetail = 50.00m, PriceSuggested = 50.00m, PricePublic = 50.00m, StockQuantity = 20, IsVegan = true, IsActive = true },
            new Product { ProductName = "Salchicha Viena 470g", Emoji = "🌭", PriceCost = 65.00m, PriceRetail = 72.00m, PriceSuggested = 72.00m, PricePublic = 72.00m, StockQuantity = 15, IsVegan = true, IsActive = true },
            new Product { ProductName = "Salchicha Parrillera 500g", Emoji = "🔥", PriceCost = 65.00m, PriceRetail = 71.00m, PriceSuggested = 71.00m, PricePublic = 71.00m, StockQuantity = 15, IsVegan = true, IsActive = true },
            new Product { ProductName = "Chorizo Verde 400g (S/F)", Emoji = "🌿", PriceCost = 80.00m, PriceRetail = 86.00m, PriceSuggested = 86.00m, PricePublic = 86.00m, StockQuantity = 10, IsVegan = true, IsActive = true, Description = "Sin frigir" },
            new Product { ProductName = "Chorizo Rojo 500g (S/F)", Emoji = "🌶️", PriceCost = 55.00m, PriceRetail = 96.00m, PriceSuggested = 96.00m, PricePublic = 96.00m, StockQuantity = 10, IsVegan = true, IsActive = true, Description = "Sin frigir" },
            new Product { ProductName = "Queso Oaxaca 500g", Emoji = "🧀", PriceCost = 88.00m, PriceRetail = 117.00m, PriceSuggested = 117.00m, PricePublic = 117.00m, StockQuantity = 20, IsVegan = true, IsActive = true },
            new Product { ProductName = "Queso Mozzarella 500g", Emoji = "🧀", PriceCost = 88.00m, PriceRetail = 117.00m, PriceSuggested = 117.00m, PricePublic = 117.00m, StockQuantity = 20, IsVegan = true, IsActive = true },
            new Product { ProductName = "Queso Oaxaca 1kg", Emoji = "🧀", PriceCost = 163.00m, PriceRetail = 203.00m, PriceSuggested = 203.00m, PricePublic = 203.00m, StockQuantity = 10, IsVegan = true, IsActive = true },
            new Product { ProductName = "Proteína en Polvo", Emoji = "💪", PriceCost = 220.00m, PriceRetail = 260.00m, PriceSuggested = 260.00m, PricePublic = 260.00m, StockQuantity = 12, IsVegan = true, IsActive = true },
            new Product { ProductName = "Queso Mozzarella 1kg", Emoji = "🧀", PriceCost = 163.00m, PriceRetail = 203.00m, PriceSuggested = 203.00m, PricePublic = 203.00m, StockQuantity = 10, IsVegan = true, IsActive = true },
            new Product { ProductName = "1/4 Setas", Emoji = "🍄", PriceCost = 50.00m, PriceRetail = 65.00m, PriceSuggested = 65.00m, PricePublic = 65.00m, StockQuantity = 30, IsVegan = true, IsActive = true },
            new Product { ProductName = "1/2 Setas", Emoji = "🍄", PriceCost = 100.00m, PriceRetail = 123.00m, PriceSuggested = 123.00m, PricePublic = 123.00m, StockQuantity = 20, IsVegan = true, IsActive = true },
            new Product { ProductName = "Kombucha Beja", Emoji = "🧃", PriceCost = 55.00m, PriceRetail = 55.00m, PriceSuggested = 55.00m, PricePublic = 55.00m, StockQuantity = 40, IsVegan = true, IsActive = true },
            new Product { ProductName = "Queso Crema", Emoji = "🥛", PriceCost = 50.00m, PriceRetail = 91.00m, PriceSuggested = 91.00m, PricePublic = 91.00m, StockQuantity = 25, IsVegan = true, IsActive = true },
            new Product { ProductName = "Parmesano", Emoji = "🧀", PriceCost = 50.00m, PriceRetail = 175.00m, PriceSuggested = 175.00m, PricePublic = 175.00m, StockQuantity = 15, IsVegan = true, IsActive = true },
            new Product { ProductName = "Mayonesa o Panela", Emoji = "🥛", PriceCost = 50.00m, PriceRetail = 70.00m, PriceSuggested = 70.00m, PricePublic = 70.00m, StockQuantity = 35, IsVegan = true, IsActive = true },
            new Product { ProductName = "Cajeta 400ml", Emoji = "🍯", PriceCost = 50.00m, PriceRetail = 105.00m, PriceSuggested = 105.00m, PricePublic = 105.00m, StockQuantity = 18, IsVegan = true, IsActive = true }
        };

        context.Products.AddRange(coreProducts);
        await context.SaveChangesAsync();

        Console.WriteLine($"✅ {coreProducts.Count} productos veganos base agregados exitosamente.");
        Console.WriteLine("🌱 Base de datos lista para operaciones de distribución vegana!");
    }

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
            LoginPasswordHash = passwordService.HashPassword(Environment.GetEnvironmentVariable("MONA_DONA_PASSWORD") ?? 
                throw new InvalidOperationException("MONA_DONA_PASSWORD environment variable is required")),
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