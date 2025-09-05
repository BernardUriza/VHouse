using VHouse.Domain.Entities;

namespace VHouse.Application.Services;

public interface IClientInventoryService
{
    Task<List<ClientProduct>> GetClientInventoryAsync(int clientTenantId);
    Task<ClientProduct?> GetClientProductAsync(int clientTenantId, int productId);
    Task<bool> AssignProductToClientAsync(int clientTenantId, int productId, decimal customPrice, int minOrderQuantity = 1);
    Task<bool> UpdateClientProductAsync(int clientTenantId, int productId, decimal customPrice, int minOrderQuantity, bool isAvailable);
    Task<bool> RemoveProductFromClientAsync(int clientTenantId, int productId);
    Task<List<Product>> GetAvailableProductsForClientAsync(int clientTenantId);
    Task<bool> IsProductAvailableForClientAsync(int clientTenantId, int productId, int quantity);
}