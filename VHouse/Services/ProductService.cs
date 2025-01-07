using System.Text.Json;
using VHouse;

public class ProductService
{
    private readonly IWebHostEnvironment _environment;
    private readonly string _filePath;

    public ProductService(IWebHostEnvironment environment)
    {
        _environment = environment;
        _filePath = Path.Combine(_environment.WebRootPath, "data", "products.json");
    }

    public async Task<List<Product>> GetProductsAsync()
    {
        if (!File.Exists(_filePath))
        {
            // Si no existe el archivo, crea uno vacío
            await SaveProductsAsync(new List<Product>());
        }

        var json = await File.ReadAllTextAsync(_filePath);
        return JsonSerializer.Deserialize<List<Product>>(json) ?? new List<Product>();
    }

    public async Task SaveProductsAsync(List<Product> products)
    {
        var json = JsonSerializer.Serialize(products, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(_filePath, json);
    }

    public async Task AddProductAsync(Product product)
    {
        var products = await GetProductsAsync();
        product.ProductId = products.Any() ? products.Max(p => p.ProductId) + 1 : 1;
        products.Add(product);
        await SaveProductsAsync(products);
    }

    public async Task UpdateProductAsync(Product updatedProduct)
    {
        var products = await GetProductsAsync();
        var product = products.FirstOrDefault(p => p.ProductId == updatedProduct.ProductId);
        if (product != null)
        {
            product.ProductName = updatedProduct.ProductName;
            product.PricePublic = updatedProduct.PricePublic; // Actualiza Precio Público
            product.PriceRetail = updatedProduct.PriceRetail; // Actualiza Precio Punto de Venta
            product.PriceCost = updatedProduct.PriceCost;     // Actualiza Precio de Costo

            // Guarda los cambios en la lista de productos
            await SaveProductsAsync(products);
        }

    }

    public async Task DeleteProductAsync(int productId)
    {
        var products = await GetProductsAsync();
        var product = products.FirstOrDefault(p => p.ProductId == productId);
        if (product != null)
        {
            products.Remove(product);
            await SaveProductsAsync(products);
        }
    }
}
