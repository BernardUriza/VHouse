using Microsoft.EntityFrameworkCore;
using VHouse;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class ProductService
{
    private readonly ApplicationDbContext _context;

    public ProductService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Product>> GetProductsAsync()
    {
        return await _context.Products.ToListAsync();
    }

    public async Task AddProductAsync(Product product)
    {
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateProductAsync(Product updatedProduct)
    {
        var product = await _context.Products.FindAsync(updatedProduct.ProductId);
        if (product != null)
        {
            product.ProductName = updatedProduct.ProductName;
            product.PricePublic = updatedProduct.PricePublic;
            product.PriceRetail = updatedProduct.PriceRetail;
            product.PriceCost = updatedProduct.PriceCost;

            await _context.SaveChangesAsync();
        }
    }

    public async Task DeleteProductAsync(int productId)
    {
        var product = await _context.Products.FindAsync(productId);
        if (product != null)
        {
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
        }
    }
}
