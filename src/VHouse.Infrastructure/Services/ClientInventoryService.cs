using Microsoft.EntityFrameworkCore;
using VHouse.Application.Services;
using VHouse.Domain.Entities;
using VHouse.Infrastructure.Data;

namespace VHouse.Infrastructure.Services;

public class ClientInventoryService : IClientInventoryService
{
    private readonly VHouseDbContext _context;

    public ClientInventoryService(VHouseDbContext context)
    {
        _context = context;
    }

    public async Task<List<ClientProduct>> GetClientInventoryAsync(int clientTenantId)
    {
        return await _context.ClientProducts
            .Include(cp => cp.Product)
            .Include(cp => cp.ClientTenant)
            .Where(cp => cp.ClientTenantId == clientTenantId && cp.IsAvailable)
            .OrderBy(cp => cp.Product.ProductName)
            .ToListAsync();
    }

    public async Task<ClientProduct?> GetClientProductAsync(int clientTenantId, int productId)
    {
        return await _context.ClientProducts
            .Include(cp => cp.Product)
            .Include(cp => cp.ClientTenant)
            .FirstOrDefaultAsync(cp => cp.ClientTenantId == clientTenantId && cp.ProductId == productId);
    }

    public async Task<bool> AssignProductToClientAsync(int clientTenantId, int productId, decimal customPrice, int minOrderQuantity = 1)
    {
        // Check if product already exists for this client
        var existingProduct = await GetClientProductAsync(clientTenantId, productId);
        if (existingProduct != null)
        {
            return false; // Product already assigned
        }

        // Verify client and product exist
        var client = await _context.ClientTenants.FindAsync(clientTenantId);
        var product = await _context.Products.FindAsync(productId);
        
        if (client == null || product == null)
        {
            return false;
        }

        var clientProduct = new ClientProduct
        {
            ClientTenantId = clientTenantId,
            ProductId = productId,
            CustomPrice = customPrice,
            MinOrderQuantity = minOrderQuantity,
            IsAvailable = true,
            AssignedAt = DateTime.UtcNow
        };

        _context.ClientProducts.Add(clientProduct);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> UpdateClientProductAsync(int clientTenantId, int productId, decimal customPrice, int minOrderQuantity, bool isAvailable)
    {
        var clientProduct = await GetClientProductAsync(clientTenantId, productId);
        if (clientProduct == null)
        {
            return false;
        }

        clientProduct.CustomPrice = customPrice;
        clientProduct.MinOrderQuantity = minOrderQuantity;
        clientProduct.IsAvailable = isAvailable;

        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> RemoveProductFromClientAsync(int clientTenantId, int productId)
    {
        var clientProduct = await GetClientProductAsync(clientTenantId, productId);
        if (clientProduct == null)
        {
            return false;
        }

        _context.ClientProducts.Remove(clientProduct);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<List<Product>> GetAvailableProductsForClientAsync(int clientTenantId)
    {
        // If client is 0 (walk-in), return all products
        if (clientTenantId == 0)
        {
            return await _context.Products
                .Where(p => p.IsActive && p.StockQuantity > 0)
                .OrderBy(p => p.ProductName)
                .ToListAsync();
        }

        // For specific clients, return only assigned products
        var clientProducts = await _context.ClientProducts
            .Include(cp => cp.Product)
            .Where(cp => cp.ClientTenantId == clientTenantId && 
                        cp.IsAvailable && 
                        cp.Product.IsActive && 
                        cp.Product.StockQuantity > 0)
            .Select(cp => cp.Product)
            .OrderBy(p => p.ProductName)
            .ToListAsync();

        return clientProducts;
    }

    public async Task<bool> IsProductAvailableForClientAsync(int clientTenantId, int productId, int quantity)
    {
        // Check if product has enough stock
        var product = await _context.Products.FindAsync(productId);
        if (product == null || !product.IsActive || product.StockQuantity < quantity)
        {
            return false;
        }

        // If walk-in client, product is available if it has stock
        if (clientTenantId == 0)
        {
            return true;
        }

        // For specific clients, check if product is assigned and available
        var clientProduct = await GetClientProductAsync(clientTenantId, productId);
        if (clientProduct == null || !clientProduct.IsAvailable)
        {
            return false;
        }

        // Check minimum order quantity
        return quantity >= clientProduct.MinOrderQuantity;
    }
}