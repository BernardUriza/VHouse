using Microsoft.EntityFrameworkCore;
using VHouse.Classes;
using VHouse.Interfaces;
using VHouse.Extensions;

namespace VHouse.Services
{
    /// <summary>
    /// Service for real-time inventory synchronization across distribution centers.
    /// </summary>
    public class InventorySynchronizationService : IInventorySynchronizationService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<InventorySynchronizationService> _logger;
        private readonly ICachingService _cachingService;

        public InventorySynchronizationService(
            ApplicationDbContext context,
            ILogger<InventorySynchronizationService> logger,
            ICachingService cachingService)
        {
            _context = context;
            _logger = logger;
            _cachingService = cachingService;
        }

        /// <summary>
        /// Synchronizes inventory levels across all distribution centers for a tenant.
        /// </summary>
        public async Task<InventorySyncResult> SynchronizeInventoryAsync(int tenantId)
        {
            var startTime = DateTime.UtcNow;
            var result = new InventorySyncResult
            {
                TenantId = tenantId,
                SynchronizedAt = startTime
            };

            try
            {
                _logger.LogInformation("Starting inventory synchronization for tenant {TenantId}", tenantId);

                // Get all warehouses for the tenant
                var warehouses = await _context.Warehouses
                    .AsNoTracking()
                    .Include(w => w.DistributionCenter)
                    .Where(w => w.DistributionCenter!.TenantId == tenantId && w.IsActive)
                    .ToListAsync();

                result.WarehousesProcessed = warehouses.Count;

                // Get all products with inventory in these warehouses
                var products = await _context.WarehouseInventories
                    .AsNoTracking()
                    .Include(wi => wi.Product)
                    .Where(wi => warehouses.Select(w => w.WarehouseId).Contains(wi.WarehouseId))
                    .Select(wi => wi.Product)
                    .Distinct()
                    .ToListAsync();

                result.ProductsProcessed = products.Count;

                // Detect and resolve conflicts
                var conflicts = new List<InventorySyncConflict>();
                foreach (var product in products)
                {
                    var productConflicts = await DetectInventoryConflictsAsync(product.ProductId, warehouses.Select(w => w.WarehouseId).ToList());
                    conflicts.AddRange(productConflicts);
                }

                result.ConflictsDetected = conflicts.Count;

                // Auto-resolve simple conflicts
                int resolvedConflicts = 0;
                foreach (var conflict in conflicts.Where(c => c.Severity <= ConflictSeverity.Medium))
                {
                    var autoResolution = new InventoryConflictResolution
                    {
                        ConflictId = conflict.ConflictId,
                        Strategy = ResolutionStrategy.UseLatestTimestamp,
                        ResolvedBy = "System",
                        ResolvedAt = DateTime.UtcNow
                    };

                    if (await ResolveSyncConflictAsync(conflict.ConflictId, autoResolution))
                    {
                        resolvedConflicts++;
                    }
                }

                result.ConflictsResolved = resolvedConflicts;

                // Update cache with synchronized inventory
                await UpdateInventoryCacheAsync(tenantId);

                result.Success = true;
                result.ProcessingTime = DateTime.UtcNow - startTime;

                _logger.LogInformation("Inventory synchronization completed for tenant {TenantId}. Processed {WarehousesCount} warehouses, {ProductsCount} products, resolved {ConflictsResolved}/{ConflictsDetected} conflicts in {ProcessingTime}",
                    tenantId, result.WarehousesProcessed, result.ProductsProcessed, result.ConflictsResolved, result.ConflictsDetected, result.ProcessingTime);

                return result;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Errors.Add(ex.Message);
                result.ProcessingTime = DateTime.UtcNow - startTime;

                _logger.LogError(ex, "Error during inventory synchronization for tenant {TenantId}", tenantId);
                return result;
            }
        }

        /// <summary>
        /// Synchronizes inventory for a specific product across all warehouses.
        /// </summary>
        public async Task<bool> SynchronizeProductInventoryAsync(int productId)
        {
            try
            {
                var product = await _context.Products
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.ProductId == productId);

                if (product == null)
                {
                    _logger.LogWarning("Product {ProductId} not found for synchronization", productId);
                    return false;
                }

                // Get all warehouse inventories for this product
                var inventories = await _context.WarehouseInventories
                    .Where(wi => wi.ProductId == productId)
                    .ToListAsync();

                // Update last synchronized timestamp
                foreach (var inventory in inventories)
                {
                    inventory.LastUpdated = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();

                // Clear cache for this product
                await _cachingService.RemoveByPatternAsync($"inventory:product:{productId}:*");

                _logger.LogInformation("Synchronized inventory for product {ProductId} across {WarehouseCount} warehouses", 
                    productId, inventories.Count);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error synchronizing inventory for product {ProductId}", productId);
                return false;
            }
        }

        /// <summary>
        /// Gets real-time inventory levels across all distribution centers.
        /// </summary>
        public async Task<List<DistributionCenterInventory>> GetRealTimeInventoryAsync(int tenantId, int? productId = null)
        {
            try
            {
                var cacheKey = $"realtime_inventory:{tenantId}:{productId ?? 0}";
                var cachedResult = await _cachingService.GetAsync<List<DistributionCenterInventory>>(cacheKey);
                
                if (cachedResult != null)
                {
                    return cachedResult;
                }

                var query = _context.DistributionCenters
                    .AsNoTracking()
                    .Include(dc => dc.Warehouses)
                    .ThenInclude(w => w.InventoryItems)
                    .ThenInclude(wi => wi.Product)
                    .Where(dc => dc.TenantId == tenantId && dc.IsActive);

                var distributionCenters = await query.ToListAsync();

                var result = new List<DistributionCenterInventory>();

                foreach (var dc in distributionCenters)
                {
                    var dcInventory = new DistributionCenterInventory
                    {
                        DistributionCenterId = dc.DistributionCenterId,
                        DistributionCenterName = dc.Name,
                        CenterCode = dc.CenterCode,
                        LastUpdated = DateTime.UtcNow
                    };

                    foreach (var warehouse in dc.Warehouses.Where(w => w.IsActive))
                    {
                        var warehouseInventory = new WarehouseInventoryLevel
                        {
                            WarehouseId = warehouse.WarehouseId,
                            WarehouseName = warehouse.Name,
                            WarehouseCode = warehouse.Code,
                            IsOnline = true,
                            LastSyncTime = DateTime.UtcNow
                        };

                        var inventoryQuery = warehouse.InventoryItems.AsQueryable();
                        if (productId.HasValue)
                        {
                            inventoryQuery = inventoryQuery.Where(wi => wi.ProductId == productId.Value);
                        }

                        foreach (var inventory in inventoryQuery)
                        {
                            var productLevel = new ProductInventoryLevel
                            {
                                ProductId = inventory.ProductId,
                                ProductName = inventory.Product?.ProductName ?? "Unknown Product",
                                SKU = inventory.Product?.SKU ?? "",
                                QuantityOnHand = inventory.QuantityOnHand,
                                ReservedQuantity = inventory.ReservedQuantity,
                                MinimumLevel = inventory.MinimumLevel,
                                MaximumLevel = inventory.MaximumLevel,
                                ReorderPoint = inventory.ReorderPoint,
                                UnitCost = inventory.UnitCost,
                                LastMovementDate = inventory.LastUpdated,
                                LastCountDate = inventory.LastCountDate ?? inventory.LastUpdated
                            };

                            warehouseInventory.Products.Add(productLevel);
                        }

                        dcInventory.Warehouses.Add(warehouseInventory);
                    }

                    dcInventory.TotalProducts = dcInventory.Warehouses.SelectMany(w => w.Products).Select(p => p.ProductId).Distinct().Count();
                    dcInventory.TotalQuantity = dcInventory.Warehouses.SelectMany(w => w.Products).Sum(p => p.QuantityOnHand);
                    dcInventory.TotalValue = dcInventory.Warehouses.SelectMany(w => w.Products).Sum(p => p.TotalValue);

                    result.Add(dcInventory);
                }

                // Cache result for 5 minutes
                await _cachingService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(5));

                _logger.LogInformation("Retrieved real-time inventory for tenant {TenantId} across {CenterCount} distribution centers", 
                    tenantId, result.Count);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving real-time inventory for tenant {TenantId}", tenantId);
                return new List<DistributionCenterInventory>();
            }
        }

        /// <summary>
        /// Updates inventory levels and triggers synchronization.
        /// </summary>
        public async Task<bool> UpdateInventoryLevelAsync(int warehouseId, int productId, int newQuantity, string reason)
        {
            try
            {
                var inventory = await _context.WarehouseInventories
                    .FirstOrDefaultAsync(wi => wi.WarehouseId == warehouseId && wi.ProductId == productId);

                if (inventory == null)
                {
                    _logger.LogWarning("Inventory record not found for warehouse {WarehouseId} and product {ProductId}", warehouseId, productId);
                    return false;
                }

                var oldQuantity = inventory.QuantityOnHand;
                inventory.QuantityOnHand = newQuantity;
                inventory.LastUpdated = DateTime.UtcNow;

                // Create movement record
                var movement = new InventoryMovement
                {
                    WarehouseId = warehouseId,
                    ProductId = productId,
                    MovementType = newQuantity > oldQuantity ? "Inbound" : "Outbound",
                    QuantityBefore = oldQuantity,
                    QuantityChange = newQuantity - oldQuantity,
                    QuantityAfter = newQuantity,
                    Reason = reason,
                    MovementDate = DateTime.UtcNow,
                    IsSynchronized = false,
                    UserId = "System",
                    UserName = "System"
                };

                await SaveMovementRecordAsync(movement);

                await _context.SaveChangesAsync();

                // Trigger synchronization for this product
                await SynchronizeProductInventoryAsync(productId);

                _logger.LogInformation("Updated inventory level for warehouse {WarehouseId}, product {ProductId}: {OldQuantity} -> {NewQuantity}, reason: {Reason}",
                    warehouseId, productId, oldQuantity, newQuantity, reason);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating inventory level for warehouse {WarehouseId}, product {ProductId}", warehouseId, productId);
                return false;
            }
        }

        /// <summary>
        /// Gets inventory movement history for auditing and synchronization tracking.
        /// </summary>
        public async Task<PagedResult<InventoryMovement>> GetInventoryMovementHistoryAsync(int tenantId, DateTime? fromDate = null, DateTime? toDate = null, int page = 1, int pageSize = 50)
        {
            try
            {
                fromDate ??= DateTime.UtcNow.AddDays(-30);
                toDate ??= DateTime.UtcNow;

                var cacheKey = $"movement_history:{tenantId}:{fromDate:yyyyMMdd}:{toDate:yyyyMMdd}:{page}:{pageSize}";
                var cachedResult = await _cachingService.GetAsync<PagedResult<InventoryMovement>>(cacheKey);
                
                if (cachedResult != null)
                {
                    return cachedResult;
                }

                // Get movements for warehouses in this tenant
                var warehouseIds = await _context.Warehouses
                    .AsNoTracking()
                    .Include(w => w.DistributionCenter)
                    .Where(w => w.DistributionCenter!.TenantId == tenantId)
                    .Select(w => w.WarehouseId)
                    .ToListAsync();

                var movements = await GetMovementRecordsAsync(warehouseIds, fromDate.Value, toDate.Value, page, pageSize);

                // Cache for 10 minutes
                await _cachingService.SetAsync(cacheKey, movements, TimeSpan.FromMinutes(10));

                _logger.LogInformation("Retrieved {MovementCount} inventory movements for tenant {TenantId} from {FromDate} to {ToDate}",
                    movements.Items.Count, tenantId, fromDate.ToString(), toDate.ToString());

                return movements;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving inventory movement history for tenant {TenantId}", tenantId);
                return new PagedResult<InventoryMovement> { Items = new List<InventoryMovement>() };
            }
        }

        /// <summary>
        /// Transfers inventory between warehouses with automatic synchronization.
        /// </summary>
        public async Task<InventoryTransferResult> TransferInventoryAsync(InventoryTransferRequest request)
        {
            var result = new InventoryTransferResult
            {
                Request = request,
                ProcessedAt = DateTime.UtcNow
            };

            try
            {
                // Validate warehouses exist and are active
                var fromWarehouse = await _context.Warehouses.FindAsync(request.FromWarehouseId);
                var toWarehouse = await _context.Warehouses.FindAsync(request.ToWarehouseId);

                if (fromWarehouse == null || !fromWarehouse.IsActive)
                {
                    result.Errors.Add($"Source warehouse {request.FromWarehouseId} not found or inactive");
                    return result;
                }

                if (toWarehouse == null || !toWarehouse.IsActive)
                {
                    result.Errors.Add($"Destination warehouse {request.ToWarehouseId} not found or inactive");
                    return result;
                }

                // Check source inventory availability
                var sourceInventory = await _context.WarehouseInventories
                    .FirstOrDefaultAsync(wi => wi.WarehouseId == request.FromWarehouseId && wi.ProductId == request.ProductId);

                if (sourceInventory == null || sourceInventory.QuantityOnHand < request.Quantity)
                {
                    result.Errors.Add($"Insufficient inventory in source warehouse. Available: {sourceInventory?.QuantityOnHand ?? 0}, Requested: {request.Quantity}");
                    return result;
                }

                // Get or create destination inventory record
                var destinationInventory = await _context.WarehouseInventories
                    .FirstOrDefaultAsync(wi => wi.WarehouseId == request.ToWarehouseId && wi.ProductId == request.ProductId);

                if (destinationInventory == null)
                {
                    destinationInventory = new WarehouseInventory
                    {
                        WarehouseId = request.ToWarehouseId,
                        ProductId = request.ProductId,
                        QuantityOnHand = 0,
                        ReservedQuantity = 0,
                        UnitCost = sourceInventory.UnitCost,
                        LastUpdated = DateTime.UtcNow
                    };
                    _context.WarehouseInventories.Add(destinationInventory);
                }

                // Perform transfer
                sourceInventory.QuantityOnHand -= request.Quantity;
                sourceInventory.LastUpdated = DateTime.UtcNow;

                destinationInventory.QuantityOnHand += request.Quantity;
                destinationInventory.LastUpdated = DateTime.UtcNow;

                // Create movement records
                var outboundMovement = new InventoryMovement
                {
                    WarehouseId = request.FromWarehouseId,
                    ProductId = request.ProductId,
                    MovementType = "Transfer",
                    QuantityBefore = sourceInventory.QuantityOnHand + request.Quantity,
                    QuantityChange = -request.Quantity,
                    QuantityAfter = sourceInventory.QuantityOnHand,
                    Reason = request.Reason,
                    Reference = request.Reference,
                    UserId = request.UserId,
                    UserName = request.UserId, // Simplified
                    MovementDate = DateTime.UtcNow,
                    IsSynchronized = true
                };

                var inboundMovement = new InventoryMovement
                {
                    WarehouseId = request.ToWarehouseId,
                    ProductId = request.ProductId,
                    MovementType = "Transfer",
                    QuantityBefore = destinationInventory.QuantityOnHand - request.Quantity,
                    QuantityChange = request.Quantity,
                    QuantityAfter = destinationInventory.QuantityOnHand,
                    Reason = request.Reason,
                    Reference = request.Reference,
                    UserId = request.UserId,
                    UserName = request.UserId, // Simplified
                    MovementDate = DateTime.UtcNow,
                    IsSynchronized = true
                };

                await SaveMovementRecordAsync(outboundMovement);
                await SaveMovementRecordAsync(inboundMovement);

                await _context.SaveChangesAsync();

                // Trigger synchronization
                await SynchronizeProductInventoryAsync(request.ProductId);

                result.Success = true;
                result.Status = "Completed";
                result.TransferId = Random.Shared.Next(10000, 99999); // Simplified ID generation
                result.Message = $"Successfully transferred {request.Quantity} units from warehouse {request.FromWarehouseId} to {request.ToWarehouseId}";

                _logger.LogInformation("Inventory transfer completed: {Quantity} units of product {ProductId} from warehouse {FromWarehouseId} to {ToWarehouseId}",
                    request.Quantity, request.ProductId, request.FromWarehouseId, request.ToWarehouseId);

                return result;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Status = "Failed";
                result.Errors.Add(ex.Message);

                _logger.LogError(ex, "Error during inventory transfer from warehouse {FromWarehouseId} to {ToWarehouseId}",
                    request.FromWarehouseId, request.ToWarehouseId);

                return result;
            }
        }

        /// <summary>
        /// Gets inventory synchronization conflicts that need resolution.
        /// </summary>
        public async Task<List<InventorySyncConflict>> GetSyncConflictsAsync(int tenantId)
        {
            try
            {
                // In a real implementation, this would query a conflicts table
                // For now, we'll simulate conflict detection
                var conflicts = new List<InventorySyncConflict>();

                var warehouses = await _context.Warehouses
                    .AsNoTracking()
                    .Include(w => w.DistributionCenter)
                    .Where(w => w.DistributionCenter!.TenantId == tenantId && w.IsActive)
                    .ToListAsync();

                // Check for quantity mismatches (simplified logic)
                var products = await _context.WarehouseInventories
                    .AsNoTracking()
                    .Include(wi => wi.Product)
                    .Where(wi => warehouses.Select(w => w.WarehouseId).Contains(wi.WarehouseId))
                    .GroupBy(wi => wi.ProductId)
                    .Where(g => g.Count() > 1)
                    .ToListAsync();

                int conflictId = 1;
                foreach (var productGroup in products)
                {
                    var inventories = productGroup.ToList();
                    var avgQuantity = inventories.Average(i => i.QuantityOnHand);
                    var hasVariance = inventories.Any(i => Math.Abs(i.QuantityOnHand - avgQuantity) > (avgQuantity * 0.1)); // 10% variance threshold

                    if (hasVariance)
                    {
                        var product = inventories.First().Product;
                        var conflict = new InventorySyncConflict
                        {
                            ConflictId = conflictId++,
                            TenantId = tenantId,
                            ProductId = product!.ProductId,
                            ProductName = product.ProductName,
                            SKU = product.SKU,
                            Type = ConflictType.QuantityMismatch,
                            Description = $"Quantity variance detected across warehouses for {product.ProductName}",
                            DetectedAt = DateTime.UtcNow,
                            Severity = GetConflictSeverity(inventories.Max(i => i.QuantityOnHand) - inventories.Min(i => i.QuantityOnHand)),
                            SuggestedResolution = "Perform physical count and update quantities",
                            IsResolved = false
                        };

                        conflict.ConflictingRecords = inventories.Select(i => new ConflictingInventory
                        {
                            WarehouseId = i.WarehouseId,
                            WarehouseName = warehouses.First(w => w.WarehouseId == i.WarehouseId).Name,
                            Quantity = i.QuantityOnHand,
                            UnitCost = i.UnitCost,
                            LastUpdated = i.LastUpdated,
                            Source = "System"
                        }).ToList();

                        conflicts.Add(conflict);
                    }
                }

                _logger.LogInformation("Found {ConflictCount} synchronization conflicts for tenant {TenantId}", conflicts.Count, tenantId);

                return conflicts;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving sync conflicts for tenant {TenantId}", tenantId);
                return new List<InventorySyncConflict>();
            }
        }

        /// <summary>
        /// Resolves inventory synchronization conflicts.
        /// </summary>
        public async Task<bool> ResolveSyncConflictAsync(int conflictId, InventoryConflictResolution resolution)
        {
            try
            {
                // In a real implementation, this would update the conflicts table
                // For now, we'll simulate conflict resolution
                _logger.LogInformation("Resolving sync conflict {ConflictId} using strategy {Strategy}", conflictId, resolution.Strategy);

                // Apply resolution strategy (simplified implementation)
                switch (resolution.Strategy)
                {
                    case ResolutionStrategy.UseLatestTimestamp:
                        _logger.LogInformation("Applied latest timestamp resolution for conflict {ConflictId}", conflictId);
                        break;
                    case ResolutionStrategy.ManualOverride:
                        _logger.LogInformation("Applied manual override resolution for conflict {ConflictId}: Quantity={Quantity}, Cost={Cost}", 
                            conflictId, resolution.ManualQuantity, resolution.ManualUnitCost);
                        break;
                    default:
                        _logger.LogInformation("Applied {Strategy} resolution for conflict {ConflictId}", resolution.Strategy, conflictId);
                        break;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resolving sync conflict {ConflictId}", conflictId);
                return false;
            }
        }

        /// <summary>
        /// Gets inventory levels below minimum thresholds across all distribution centers.
        /// </summary>
        public async Task<List<LowStockAlert>> GetLowStockAlertsAsync(int tenantId)
        {
            try
            {
                var cacheKey = $"low_stock_alerts:{tenantId}";
                var cachedResult = await _cachingService.GetAsync<List<LowStockAlert>>(cacheKey);
                
                if (cachedResult != null)
                {
                    return cachedResult;
                }

                var lowStockItems = await _context.WarehouseInventories
                    .AsNoTracking()
                    .Include(wi => wi.Product)
                    .Include(wi => wi.Warehouse)
                    .ThenInclude(w => w!.DistributionCenter)
                    .Where(wi => wi.Warehouse!.DistributionCenter!.TenantId == tenantId &&
                                wi.QuantityOnHand <= wi.ReorderPoint &&
                                wi.Warehouse.IsActive)
                    .ToListAsync();

                var alerts = lowStockItems.Select(wi => new LowStockAlert
                {
                    ProductId = wi.ProductId,
                    ProductName = wi.Product?.ProductName ?? "Unknown Product",
                    SKU = wi.Product?.SKU ?? "",
                    WarehouseId = wi.WarehouseId,
                    WarehouseName = wi.Warehouse?.Name ?? "Unknown Warehouse",
                    CurrentQuantity = wi.QuantityOnHand,
                    ReorderPoint = wi.ReorderPoint,
                    MinimumLevel = wi.MinimumLevel,
                    SuggestedOrderQuantity = Math.Max(wi.MaximumLevel - wi.QuantityOnHand, wi.ReorderPoint * 2),
                    Severity = GetAlertSeverity(wi.QuantityOnHand, wi.ReorderPoint, wi.MinimumLevel),
                    DaysOfStockRemaining = EstimateDaysOfStock(wi.QuantityOnHand, wi.ProductId),
                    LastRestock = wi.LastUpdated,
                    IsBackordered = wi.QuantityOnHand <= 0,
                    RecommendedAction = GetRecommendedAction(wi.QuantityOnHand, wi.ReorderPoint, wi.MinimumLevel)
                }).ToList();

                // Cache for 15 minutes
                await _cachingService.SetAsync(cacheKey, alerts, TimeSpan.FromMinutes(15));

                _logger.LogInformation("Found {AlertCount} low stock alerts for tenant {TenantId}", alerts.Count, tenantId);

                return alerts;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving low stock alerts for tenant {TenantId}", tenantId);
                return new List<LowStockAlert>();
            }
        }

        /// <summary>
        /// Performs automatic rebalancing of inventory across distribution centers.
        /// </summary>
        public async Task<InventoryRebalanceResult> RebalanceInventoryAsync(int tenantId, RebalanceStrategy strategy)
        {
            var startTime = DateTime.UtcNow;
            var result = new InventoryRebalanceResult
            {
                TenantId = tenantId,
                Strategy = strategy,
                ProcessedAt = startTime
            };

            try
            {
                _logger.LogInformation("Starting inventory rebalancing for tenant {TenantId} using strategy {Strategy}", tenantId, strategy);

                // Get all warehouses for the tenant
                var warehouses = await _context.Warehouses
                    .AsNoTracking()
                    .Include(w => w.DistributionCenter)
                    .Include(w => w.InventoryItems)
                    .ThenInclude(wi => wi.Product)
                    .Where(w => w.DistributionCenter!.TenantId == tenantId && w.IsActive)
                    .ToListAsync();

                // Analyze products for rebalancing opportunities
                var productsForRebalance = new List<int>();
                var allInventories = warehouses.SelectMany(w => w.InventoryItems).GroupBy(wi => wi.ProductId);

                foreach (var productGroup in allInventories)
                {
                    var inventories = productGroup.ToList();
                    if (inventories.Count > 1 && ShouldRebalanceProduct(inventories, strategy))
                    {
                        productsForRebalance.Add(productGroup.Key);
                    }
                }

                result.ProductsAnalyzed = allInventories.Count();
                result.ProductsRebalanced = productsForRebalance.Count;

                // Generate rebalance transfers
                var transfers = new List<RebalanceTransfer>();
                foreach (var productId in productsForRebalance)
                {
                    var productTransfers = await GenerateRebalanceTransfers(productId, warehouses, strategy);
                    transfers.AddRange(productTransfers);
                }

                result.Transfers = transfers;
                result.TotalTransferCost = transfers.Sum(t => t.TotalValue * 0.05m); // Assume 5% transfer cost
                result.TotalCostSavings = transfers.Sum(t => t.EstimatedCostSaving);
                result.ProcessingTime = DateTime.UtcNow - startTime;
                result.Summary = GenerateRebalanceSummary(result);
                result.Success = true;

                _logger.LogInformation("Inventory rebalancing completed for tenant {TenantId}. Analyzed {ProductsAnalyzed} products, generated {TransferCount} transfers with estimated savings of ${CostSavings}",
                    tenantId, result.ProductsAnalyzed, result.Transfers.Count, result.TotalCostSavings);

                return result;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ProcessingTime = DateTime.UtcNow - startTime;
                
                _logger.LogError(ex, "Error during inventory rebalancing for tenant {TenantId}", tenantId);
                return result;
            }
        }

        #region Private Helper Methods

        private async Task<List<InventorySyncConflict>> DetectInventoryConflictsAsync(int productId, List<int> warehouseIds)
        {
            // Simplified conflict detection logic
            var conflicts = new List<InventorySyncConflict>();
            
            // In a real implementation, this would check for various types of conflicts
            // such as timestamp conflicts, quantity mismatches, etc.
            
            return await Task.FromResult(conflicts);
        }

        private async Task UpdateInventoryCacheAsync(int tenantId)
        {
            // Clear all inventory-related cache entries for this tenant
            await _cachingService.RemoveByPatternAsync($"*inventory*{tenantId}*");
            await _cachingService.RemoveByPatternAsync($"realtime_inventory:{tenantId}:*");
        }

        private async Task SaveMovementRecordAsync(InventoryMovement movement)
        {
            // In a real implementation, this would save to an InventoryMovements table
            // For now, we'll just log the movement
            _logger.LogInformation("Inventory movement recorded: {MovementType} {Quantity} units of product {ProductId} in warehouse {WarehouseId}, reason: {Reason}",
                movement.MovementType, movement.QuantityChange, movement.ProductId, movement.WarehouseId, movement.Reason);
            
            await Task.CompletedTask;
        }

        private async Task<PagedResult<InventoryMovement>> GetMovementRecordsAsync(List<int> warehouseIds, DateTime fromDate, DateTime toDate, int page, int pageSize)
        {
            // In a real implementation, this would query the InventoryMovements table
            // For now, we'll return a simplified result
            var movements = new List<InventoryMovement>();
            
            return await Task.FromResult(new PagedResult<InventoryMovement>
            {
                Items = movements,
                TotalItems = 0,
                CurrentPage = page,
                PageSize = pageSize
            });
        }

        private static ConflictSeverity GetConflictSeverity(int quantityVariance)
        {
            return quantityVariance switch
            {
                <= 10 => ConflictSeverity.Low,
                <= 50 => ConflictSeverity.Medium,
                <= 100 => ConflictSeverity.High,
                _ => ConflictSeverity.Critical
            };
        }

        private static AlertSeverity GetAlertSeverity(int currentQuantity, int reorderPoint, int minimumLevel)
        {
            if (currentQuantity <= 0)
                return AlertSeverity.Emergency;
            if (currentQuantity <= minimumLevel)
                return AlertSeverity.Critical;
            if (currentQuantity <= reorderPoint)
                return AlertSeverity.Warning;
            return AlertSeverity.Info;
        }

        private static int EstimateDaysOfStock(int currentQuantity, int productId)
        {
            // Simplified calculation - assume 2 units per day average consumption
            var dailyConsumption = 2;
            return currentQuantity / Math.Max(dailyConsumption, 1);
        }

        private static string GetRecommendedAction(int currentQuantity, int reorderPoint, int minimumLevel)
        {
            if (currentQuantity <= 0)
                return "URGENT: Order immediately - out of stock";
            if (currentQuantity <= minimumLevel)
                return "CRITICAL: Place emergency order";
            if (currentQuantity <= reorderPoint)
                return "WARNING: Place regular order";
            return "MONITOR: Stock levels adequate";
        }

        private static bool ShouldRebalanceProduct(List<WarehouseInventory> inventories, RebalanceStrategy strategy)
        {
            // Simplified logic - check if there's significant imbalance
            var maxQuantity = inventories.Max(i => i.QuantityOnHand);
            var minQuantity = inventories.Min(i => i.QuantityOnHand);
            
            // Rebalance if difference is more than 50% of average
            var avgQuantity = inventories.Average(i => i.QuantityOnHand);
            return (maxQuantity - minQuantity) > (avgQuantity * 0.5);
        }

        private async Task<List<RebalanceTransfer>> GenerateRebalanceTransfers(int productId, List<Warehouse> warehouses, RebalanceStrategy strategy)
        {
            var transfers = new List<RebalanceTransfer>();
            
            // Get inventories for this product
            var productInventories = warehouses
                .SelectMany(w => w.InventoryItems.Where(wi => wi.ProductId == productId))
                .ToList();

            if (productInventories.Count < 2)
                return transfers;

            // Simple rebalancing algorithm - move from highest to lowest
            var sortedInventories = productInventories.OrderByDescending(i => i.QuantityOnHand).ToList();
            var highestInventory = sortedInventories.First();
            var lowestInventory = sortedInventories.Last();

            if (highestInventory.QuantityOnHand > lowestInventory.QuantityOnHand + 10) // Threshold
            {
                var transferQuantity = (highestInventory.QuantityOnHand - lowestInventory.QuantityOnHand) / 2;
                
                var transfer = new RebalanceTransfer
                {
                    ProductId = productId,
                    ProductName = highestInventory.Product?.ProductName ?? "Unknown",
                    SKU = highestInventory.Product?.SKU ?? "",
                    FromWarehouseId = highestInventory.WarehouseId,
                    FromWarehouseName = warehouses.First(w => w.WarehouseId == highestInventory.WarehouseId).Name,
                    ToWarehouseId = lowestInventory.WarehouseId,
                    ToWarehouseName = warehouses.First(w => w.WarehouseId == lowestInventory.WarehouseId).Name,
                    TransferQuantity = transferQuantity,
                    UnitCost = highestInventory.UnitCost,
                    TotalValue = transferQuantity * highestInventory.UnitCost,
                    Reason = $"Rebalancing using {strategy} strategy",
                    EstimatedCostSaving = transferQuantity * highestInventory.UnitCost * 0.02m, // 2% savings
                    Priority = 1,
                    IsApproved = false
                };

                transfers.Add(transfer);
            }

            return await Task.FromResult(transfers);
        }

        private static string GenerateRebalanceSummary(InventoryRebalanceResult result)
        {
            return $"Rebalancing completed using {result.Strategy} strategy. " +
                   $"Analyzed {result.ProductsAnalyzed} products, created {result.Transfers.Count} transfer recommendations. " +
                   $"Estimated savings: ${result.TotalCostSavings:F2}, Transfer cost: ${result.TotalTransferCost:F2}";
        }

        #endregion
    }
}