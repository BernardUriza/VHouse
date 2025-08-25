using VHouse;

namespace VHouse.Interfaces
{
    public interface IProductService
    {
        Task<List<Product>> GetProductsAsync();
        Task AddProductAsync(Product product);
        Task UpdateProductAsync(Product updatedProduct);
        Task DeleteProductAsync(int productId);
        Task SeedProductsAsync(IServiceScopeFactory scopeFactory);
    }
}