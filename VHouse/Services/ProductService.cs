using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using VHouse;
using VHouse.Interfaces;

public class ProductService : IProductService
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<ProductService> _logger;
    private readonly string _jsonFilePath;

    public ProductService(ApplicationDbContext context, IWebHostEnvironment env, ILogger<ProductService> logger)
    {
        _context = context;
        _env = env;
        _logger = logger;
        _jsonFilePath = Path.Combine(_env.ContentRootPath, "wwwroot/data", "products.json");
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
        _logger.LogInformation("🚀 Starting product seeding...");

        using var scope = scopeFactory.CreateScope();
        var scopedContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Debug database connection
        try
        {
            _logger.LogInformation("🟢 Checking database connection: {ConnectionString}", scopedContext.Database.GetConnectionString());
            await scopedContext.Database.CanConnectAsync();
            _logger.LogInformation("✅ Database connection successful!");
        }
        catch (Exception dbEx)
        {
            _logger.LogError(dbEx, "❌ Unable to connect to database");
            return;
        }

        // Check if products already exist
        bool productsExist = await scopedContext.Products.AnyAsync();
        _logger.LogInformation("🔎 Products already exist? {ProductsExist}", productsExist);

        if (productsExist)
        {
            _logger.LogInformation("⚠️ Skipping seeding, products already exist in the database.");
            return;
        }

        _logger.LogInformation("📂 Checking if JSON file exists at: {JsonFilePath}", _jsonFilePath);

        if (!File.Exists(_jsonFilePath))
        {
            _logger.LogError("❌ JSON file not found: {JsonFilePath}", _jsonFilePath);
            return;
        }

        try
        {
            _logger.LogInformation("📖 Reading JSON file...");
            var jsonData = await File.ReadAllTextAsync(_jsonFilePath);

            if (string.IsNullOrWhiteSpace(jsonData))
            {
                _logger.LogWarning("⚠️ JSON file is empty!");
                return;
            }

            _logger.LogInformation("📊 Deserializing JSON data...");
            var products = JsonSerializer.Deserialize<List<Product>>(jsonData);

            if (products == null || !products.Any())
            {
                _logger.LogWarning("⚠️ No products found in JSON file!");
                return;
            }

            // Ensure no null values for required fields
            foreach (var product in products)
            {
                if (string.IsNullOrWhiteSpace(product.Emoji))
                {
                    _logger.LogWarning("⚠️ Product '{ProductName}' is missing an Emoji. Assigning default emoji.", product.ProductName);
                    product.Emoji = "🌟"; // Default emoji
                }
            }

            _logger.LogInformation("✅ Adding {ProductCount} products to the database...", products.Count);
            scopedContext.Products.AddRange(products);
            await scopedContext.SaveChangesAsync();
            _logger.LogInformation("🎉 Products successfully seeded!");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Exception while loading products from JSON");
        }
    }

}
