using VHouse.Classes;

namespace VHouse.Interfaces
{
    /// <summary>
    /// Service interface for brand management.
    /// </summary>
    public interface IBrandService
    {
        Task<List<Brand>> GetBrandsAsync();
        Task<Brand?> GetBrandByIdAsync(int brandId);
        Task<List<Brand>> GetActiveBrandsAsync();
        Task AddBrandAsync(Brand brand);
        Task UpdateBrandAsync(Brand brand);
        Task DeleteBrandAsync(int brandId);
        Task<bool> BrandExistsAsync(int brandId);
    }
}