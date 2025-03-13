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
        _jsonFilePath = Path.Combine(_env.ContentRootPath, "Data", "products.json"); // ✅ Load JSON from 'Data/' folder
    }

    /// <summary>
    /// Retrieves all products from the database.
    /// </summary>
    public async Task<List<Product>> GetProductsAsync()
    {
        return await _context.Products.ToListAsync();
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
    public async Task SeedProductsAsync()
    {
        if (await _context.Products.AnyAsync()) return; // ✅ Prevent duplicate seeding

        if (File.Exists(_jsonFilePath))
        {
            try
            {
                var jsonData = await File.ReadAllTextAsync(_jsonFilePath);
                var products = JsonSerializer.Deserialize<List<Product>>(jsonData);

                if (products != null && products.Any())
                {
                    _context.Products.AddRange(products);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error loading products from JSON: {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine($"⚠️ No product JSON file found at {_jsonFilePath}");
        }
    }
}
