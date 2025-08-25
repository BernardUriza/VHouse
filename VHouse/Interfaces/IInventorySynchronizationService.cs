using VHouse.Classes;

namespace VHouse.Interfaces
{
    /// <summary>
    /// Service interface for real-time inventory synchronization across distribution centers.
    /// </summary>
    public interface IInventorySynchronizationService
    {
        /// <summary>
        /// Synchronizes inventory levels across all distribution centers for a tenant.
        /// </summary>
        Task<InventorySyncResult> SynchronizeInventoryAsync(int tenantId);

        /// <summary>
        /// Synchronizes inventory for a specific product across all warehouses.
        /// </summary>
        Task<bool> SynchronizeProductInventoryAsync(int productId);

        /// <summary>
        /// Gets real-time inventory levels across all distribution centers.
        /// </summary>
        Task<List<DistributionCenterInventory>> GetRealTimeInventoryAsync(int tenantId, int? productId = null);

        /// <summary>
        /// Updates inventory levels and triggers synchronization.
        /// </summary>
        Task<bool> UpdateInventoryLevelAsync(int warehouseId, int productId, int newQuantity, string reason);

        /// <summary>
        /// Gets inventory movement history for auditing and synchronization tracking.
        /// </summary>
        Task<PagedResult<InventoryMovement>> GetInventoryMovementHistoryAsync(int tenantId, DateTime? fromDate = null, DateTime? toDate = null, int page = 1, int pageSize = 50);

        /// <summary>
        /// Transfers inventory between warehouses with automatic synchronization.
        /// </summary>
        Task<InventoryTransferResult> TransferInventoryAsync(InventoryTransferRequest request);

        /// <summary>
        /// Gets inventory synchronization conflicts that need resolution.
        /// </summary>
        Task<List<InventorySyncConflict>> GetSyncConflictsAsync(int tenantId);

        /// <summary>
        /// Resolves inventory synchronization conflicts.
        /// </summary>
        Task<bool> ResolveSyncConflictAsync(int conflictId, InventoryConflictResolution resolution);

        /// <summary>
        /// Gets inventory levels below minimum thresholds across all distribution centers.
        /// </summary>
        Task<List<LowStockAlert>> GetLowStockAlertsAsync(int tenantId);

        /// <summary>
        /// Performs automatic rebalancing of inventory across distribution centers.
        /// </summary>
        Task<InventoryRebalanceResult> RebalanceInventoryAsync(int tenantId, RebalanceStrategy strategy);
    }

    /// <summary>
    /// Result of inventory synchronization operation.
    /// </summary>
    public class InventorySyncResult
    {
        public bool Success { get; set; }
        public int TenantId { get; set; }
        public int WarehousesProcessed { get; set; }
        public int ProductsProcessed { get; set; }
        public int ConflictsDetected { get; set; }
        public int ConflictsResolved { get; set; }
        public TimeSpan ProcessingTime { get; set; }
        public List<string> Warnings { get; set; } = new();
        public List<string> Errors { get; set; } = new();
        public DateTime SynchronizedAt { get; set; }
    }

    /// <summary>
    /// Distribution center inventory information.
    /// </summary>
    public class DistributionCenterInventory
    {
        public int DistributionCenterId { get; set; }
        public string DistributionCenterName { get; set; } = string.Empty;
        public string CenterCode { get; set; } = string.Empty;
        public List<WarehouseInventoryLevel> Warehouses { get; set; } = new();
        public int TotalProducts { get; set; }
        public int TotalQuantity { get; set; }
        public decimal TotalValue { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    /// <summary>
    /// Warehouse inventory level information.
    /// </summary>
    public class WarehouseInventoryLevel
    {
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; } = string.Empty;
        public string WarehouseCode { get; set; } = string.Empty;
        public List<ProductInventoryLevel> Products { get; set; } = new();
        public bool IsOnline { get; set; } = true;
        public DateTime LastSyncTime { get; set; }
    }

    /// <summary>
    /// Product inventory level information.
    /// </summary>
    public class ProductInventoryLevel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public int QuantityOnHand { get; set; }
        public int ReservedQuantity { get; set; }
        public int AvailableQuantity => QuantityOnHand - ReservedQuantity;
        public int MinimumLevel { get; set; }
        public int MaximumLevel { get; set; }
        public int ReorderPoint { get; set; }
        public bool IsLowStock => AvailableQuantity <= ReorderPoint;
        public decimal UnitCost { get; set; }
        public decimal TotalValue => QuantityOnHand * UnitCost;
        public DateTime LastMovementDate { get; set; }
        public DateTime LastCountDate { get; set; }
    }

    /// <summary>
    /// Inventory movement record for tracking changes.
    /// </summary>
    public class InventoryMovement
    {
        public int MovementId { get; set; }
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; } = string.Empty;
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public string MovementType { get; set; } = string.Empty; // Inbound, Outbound, Transfer, Adjustment
        public int QuantityBefore { get; set; }
        public int QuantityChange { get; set; }
        public int QuantityAfter { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string Reference { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public DateTime MovementDate { get; set; }
        public bool IsSynchronized { get; set; }
        public DateTime? SynchronizedAt { get; set; }
    }

    /// <summary>
    /// Inventory transfer request.
    /// </summary>
    public class InventoryTransferRequest
    {
        public int FromWarehouseId { get; set; }
        public int ToWarehouseId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string Reference { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public DateTime RequestedDate { get; set; } = DateTime.UtcNow;
        public bool RequireConfirmation { get; set; } = true;
    }

    /// <summary>
    /// Result of inventory transfer operation.
    /// </summary>
    public class InventoryTransferResult
    {
        public bool Success { get; set; }
        public int TransferId { get; set; }
        public InventoryTransferRequest Request { get; set; } = new();
        public string Status { get; set; } = string.Empty; // Pending, InTransit, Completed, Failed
        public DateTime ProcessedAt { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<string> Warnings { get; set; } = new();
        public List<string> Errors { get; set; } = new();
    }

    /// <summary>
    /// Inventory synchronization conflict.
    /// </summary>
    public class InventorySyncConflict
    {
        public int ConflictId { get; set; }
        public int TenantId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public ConflictType Type { get; set; }
        public string Description { get; set; } = string.Empty;
        public List<ConflictingInventory> ConflictingRecords { get; set; } = new();
        public DateTime DetectedAt { get; set; }
        public ConflictSeverity Severity { get; set; }
        public string SuggestedResolution { get; set; } = string.Empty;
        public bool IsResolved { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public string? ResolvedBy { get; set; }
    }

    /// <summary>
    /// Types of inventory synchronization conflicts.
    /// </summary>
    public enum ConflictType
    {
        QuantityMismatch,
        PriceDifference,
        LocationDiscrepancy,
        SystemConflict,
        TimestampConflict
    }

    /// <summary>
    /// Severity levels for synchronization conflicts.
    /// </summary>
    public enum ConflictSeverity
    {
        Low,
        Medium,
        High,
        Critical
    }

    /// <summary>
    /// Conflicting inventory record.
    /// </summary>
    public class ConflictingInventory
    {
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitCost { get; set; }
        public DateTime LastUpdated { get; set; }
        public string LastUpdatedBy { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
    }

    /// <summary>
    /// Inventory conflict resolution strategy.
    /// </summary>
    public class InventoryConflictResolution
    {
        public int ConflictId { get; set; }
        public ResolutionStrategy Strategy { get; set; }
        public int? AuthoritativeWarehouseId { get; set; }
        public int? ManualQuantity { get; set; }
        public decimal? ManualUnitCost { get; set; }
        public string Notes { get; set; } = string.Empty;
        public string ResolvedBy { get; set; } = string.Empty;
        public DateTime ResolvedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Resolution strategies for inventory conflicts.
    /// </summary>
    public enum ResolutionStrategy
    {
        UseLatestTimestamp,
        UseHighestQuantity,
        UseLowestQuantity,
        UseSpecificWarehouse,
        ManualOverride,
        AverageValues,
        RequirePhysicalCount
    }

    /// <summary>
    /// Low stock alert information.
    /// </summary>
    public class LowStockAlert
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; } = string.Empty;
        public int CurrentQuantity { get; set; }
        public int ReorderPoint { get; set; }
        public int MinimumLevel { get; set; }
        public int SuggestedOrderQuantity { get; set; }
        public AlertSeverity Severity { get; set; }
        public int DaysOfStockRemaining { get; set; }
        public DateTime LastRestock { get; set; }
        public bool IsBackordered { get; set; }
        public string RecommendedAction { get; set; } = string.Empty;
    }

    /// <summary>
    /// Alert severity levels.
    /// </summary>
    public enum AlertSeverity
    {
        Info,
        Warning,
        Critical,
        Emergency
    }

    /// <summary>
    /// Inventory rebalancing result.
    /// </summary>
    public class InventoryRebalanceResult
    {
        public bool Success { get; set; }
        public int TenantId { get; set; }
        public RebalanceStrategy Strategy { get; set; }
        public int ProductsAnalyzed { get; set; }
        public int ProductsRebalanced { get; set; }
        public List<RebalanceTransfer> Transfers { get; set; } = new();
        public decimal TotalCostSavings { get; set; }
        public decimal TotalTransferCost { get; set; }
        public TimeSpan ProcessingTime { get; set; }
        public string Summary { get; set; } = string.Empty;
        public DateTime ProcessedAt { get; set; }
    }

    /// <summary>
    /// Inventory rebalancing strategies.
    /// </summary>
    public enum RebalanceStrategy
    {
        MinimizeTransfers,
        OptimizeForDemand,
        EqualDistribution,
        CostOptimized,
        TurnoverOptimized
    }

    /// <summary>
    /// Rebalance transfer recommendation.
    /// </summary>
    public class RebalanceTransfer
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public int FromWarehouseId { get; set; }
        public string FromWarehouseName { get; set; } = string.Empty;
        public int ToWarehouseId { get; set; }
        public string ToWarehouseName { get; set; } = string.Empty;
        public int TransferQuantity { get; set; }
        public decimal UnitCost { get; set; }
        public decimal TotalValue { get; set; }
        public string Reason { get; set; } = string.Empty;
        public decimal EstimatedCostSaving { get; set; }
        public int Priority { get; set; }
        public bool IsApproved { get; set; }
    }
}