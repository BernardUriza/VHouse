using VHouse;
using VHouse.Classes;

namespace VHouse.Interfaces
{
    public interface IProductService
    {
        Task<List<Product>> GetProductsAsync();
        Task AddProductAsync(Product product);
        Task UpdateProductAsync(Product updatedProduct);
        Task DeleteProductAsync(int productId);
        Task SeedProductsAsync(IServiceScopeFactory scopeFactory);
        
        // Pagination and performance methods
        Task<PagedResult<Product>> GetProductsPagedAsync(PaginationParameters pagination);
        Task<PagedResult<Product>> SearchProductsAsync(string searchTerm, PaginationParameters pagination);
        Task<List<Product>> GetProductsByCategoryAsync(int? brandId, bool? isActive = true);
        Task<Product?> GetProductByIdAsync(int id);
        Task<bool> ProductExistsAsync(int id);
    }
}