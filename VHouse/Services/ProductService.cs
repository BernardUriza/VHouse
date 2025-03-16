using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using VHouse;

public class ProductService
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _env;
    private readonly string _jsonFilePath;

    public ProductService(ApplicationDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
        _jsonFilePath = Path.Combine(_env.ContentRootPath, "wwwroot/data", "products.json"); // ✅ Load JSON from 'Data/' folder
    }

    /// <summary>
    /// Retrieves all products from the database.
    /// </summary>
    public async Task<List<Product>> GetProductsAsync()
    {
        return await _context.Products
            .AsNoTracking() // ✅ Prevent tracking issues
            .ToListAsync();
    }


    /// <summary>
    /// Adds a new product to the database.
    /// </summary>
    public async Task AddProductAsync(Product product)
    {
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Updates an existing product.
    /// </summary>
    public async Task UpdateProductAsync(Product updatedProduct)
    {
        var product = await _context.Products.FindAsync(updatedProduct.ProductId);
        if (product != null)
        {
            product.ProductName = updatedProduct.ProductName;
            product.PricePublic = updatedProduct.PricePublic;
            product.PriceRetail = updatedProduct.PriceRetail;
            product.PriceCost = updatedProduct.PriceCost;
            product.PriceSuggested = updatedProduct.PriceSuggested;
            product.Emoji = updatedProduct.Emoji;

            await _context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Deletes a product from the database.
    /// </summary>
    public async Task DeleteProductAsync(int productId)
    {
        var product = await _context.Products.FindAsync(productId);
        if (product != null)
        {
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Seeds products from JSON if the database is empty.
    /// </summary>
    public async Task SeedProductsAsync(IServiceScopeFactory scopeFactory)
    {
        Console.WriteLine("🚀 Starting product seeding...");

        using var scope = scopeFactory.CreateScope();
        var scopedContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Debug database connection
        try
        {
            Console.WriteLine($"🟢 Checking DB Connection: {scopedContext.Database.GetConnectionString()}");
            await scopedContext.Database.CanConnectAsync();
            Console.WriteLine("✅ Database connection successful!");
        }
        catch (Exception dbEx)
        {
            Console.WriteLine($"❌ ERROR: Unable to connect to database: {dbEx.Message}");
            return;
        }

        // Check if products already exist
        bool productsExist = await scopedContext.Products.AnyAsync();
        Console.WriteLine($"🔎 Products already exist? {productsExist}");

        if (productsExist)
        {
            Console.WriteLine("⚠️ Skipping seeding, products already exist in the database.");
            return;
        }

        Console.WriteLine($"📂 Checking if JSON file exists at: {_jsonFilePath}");

        if (!File.Exists(_jsonFilePath))
        {
            Console.WriteLine($"❌ ERROR2: JSON file not found: {_jsonFilePath}");
            return;
        }

        try
        {
            Console.WriteLine("📖 Reading JSON file...");
            var jsonData = await File.ReadAllTextAsync(_jsonFilePath);

            if (string.IsNullOrWhiteSpace(jsonData))
            {
                Console.WriteLine("⚠️ WARNING: JSON file is empty!");
                return;
            }

            Console.WriteLine("📊 Deserializing JSON data...");
            var products = JsonSerializer.Deserialize<List<Product>>(jsonData);

            if (products == null || !products.Any())
            {
                Console.WriteLine("⚠️ WARNING: No products found in JSON file!");
                return;
            }

            // Ensure no null values for required fields
            foreach (var product in products)
            {
                if (string.IsNullOrWhiteSpace(product.Emoji))
                {
                    Console.WriteLine($"⚠️ WARNING: Product '{product.ProductName}' is missing an Emoji. Assigning default emoji.");
                    product.Emoji = "🌟"; // Default emoji
                }
            }

            Console.WriteLine($"✅ Adding {products.Count} products to the database...");
            scopedContext.Products.AddRange(products);
            await scopedContext.SaveChangesAsync();
            Console.WriteLine("🎉 Products successfully seeded!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ ERROR: Exception while loading products from JSON: {ex.Message}");
        }
    }

}
