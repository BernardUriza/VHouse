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
        _jsonFilePath = Path.Combine(_env.ContentRootPath, "wwwroot\\data", "products.json"); // ✅ Load JSON from 'Data/' folder
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
        using var scope = scopeFactory.CreateScope(); // ✅ Creates a new scoped DbContext
        var scopedContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        if (await scopedContext.Products.AnyAsync()) return; // ✅ Prevent duplicate seeding

        if (File.Exists(_jsonFilePath))
        {
            try
            {
                var jsonData = await File.ReadAllTextAsync(_jsonFilePath);
                var products = JsonSerializer.Deserialize<List<Product>>(jsonData);

                if (products != null && products.Any())
                {
                    scopedContext.Products.AddRange(products);
                    await scopedContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error loading products from JSON: {ex.Message}");
            }
        }
    }
}
