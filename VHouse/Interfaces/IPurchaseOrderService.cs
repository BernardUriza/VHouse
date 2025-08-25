using VHouse.Classes;

namespace VHouse.Interfaces
{
    /// <summary>
    /// Service interface for purchase order management.
    /// </summary>
    public interface IPurchaseOrderService
    {
        Task<List<PurchaseOrder>> GetPurchaseOrdersAsync();
        Task<PurchaseOrder?> GetPurchaseOrderByIdAsync(int purchaseOrderId);
        Task<List<PurchaseOrder>> GetPurchaseOrdersBySupplierAsync(int supplierId);
        Task<List<PurchaseOrder>> GetPurchaseOrdersByStatusAsync(string status);
        Task<PurchaseOrder> CreatePurchaseOrderAsync(PurchaseOrder purchaseOrder);
        Task UpdatePurchaseOrderAsync(PurchaseOrder purchaseOrder);
        Task DeletePurchaseOrderAsync(int purchaseOrderId);
        Task<string> GenerateOrderNumberAsync();
        Task ReceivePurchaseOrderAsync(int purchaseOrderId, Dictionary<int, int> receivedQuantities);
        Task ApprovePurchaseOrderAsync(int purchaseOrderId);
        Task CancelPurchaseOrderAsync(int purchaseOrderId);
    }
}