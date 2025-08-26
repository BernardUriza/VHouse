using VHouse.Classes;

namespace VHouse.Interfaces;

public interface IIntegrationService
{
    Task<IntegrationResult> SyncWithERPAsync(ERPSyncRequest request);
    Task<CRMSyncResult> SyncWithCRMAsync(CRMData data);
    Task<WebhookResult> ProcessWebhookAsync(WebhookPayload payload);
    Task<ConnectorHealth> MonitorConnectorHealthAsync(string connectorId);
    Task<ECommerceSync> SyncWithECommerceAsync(ECommerceSyncRequest request);
    Task<InventorySync> SyncInventoryAsync(InventorySyncRequest request);
    Task<CustomerSync> SyncCustomersAsync(CustomerSyncRequest request);
    Task<OrderSync> SyncOrdersAsync(OrderSyncRequest request);
    Task<IntegrationStatus> GetIntegrationStatusAsync(string integrationId);
    Task<DataMapping> CreateDataMappingAsync(DataMappingRequest request);
    Task<TransformationResult> TransformDataAsync(DataTransformationRequest request);
    Task<ValidationResult> ValidateIntegrationDataAsync(IntegrationValidationRequest request);
    Task<SyncSchedule> ScheduleSyncAsync(SyncScheduleRequest request);
    Task<IntegrationLog> GetIntegrationLogsAsync(string integrationId, LogFilter filter);
    Task<ConflictResolution> ResolveDataConflictAsync(ConflictResolutionRequest request);
}

public interface IERPIntegrationService
{
    Task<ERPConnection> ConnectToERPAsync(ERPConnectionConfig config);
    Task<ERPSyncResult> SyncFinancialDataAsync(FinancialDataSyncRequest request);
    Task<ERPSyncResult> SyncSupplierDataAsync(SupplierDataSyncRequest request);
    Task<ERPSyncResult> SyncPurchaseOrdersAsync(PurchaseOrderSyncRequest request);
    Task<ERPSyncResult> SyncAccountingDataAsync(AccountingDataSyncRequest request);
    Task<ERPInventory> GetERPInventoryAsync(ERPInventoryRequest request);
    Task<ERPReport> GenerateERPReportAsync(ERPReportRequest request);
    Task<ERPTransaction> CreateERPTransactionAsync(ERPTransactionRequest request);
    Task<ERPReconciliation> ReconcileERPDataAsync(ERPReconciliationRequest request);
}

public interface ICRMIntegrationService
{
    Task<CRMConnection> ConnectToCRMAsync(CRMConnectionConfig config);
    Task<CRMSyncResult> SyncContactsAsync(ContactSyncRequest request);
    Task<CRMSyncResult> SyncLeadsAsync(LeadSyncRequest request);
    Task<CRMSyncResult> SyncOpportunitiesAsync(OpportunitySyncRequest request);
    Task<CRMSyncResult> SyncActivitiesAsync(ActivitySyncRequest request);
    Task<CRMCampaign> CreateCampaignAsync(CampaignRequest request);
    Task<CRMAnalytics> GetCRMAnalyticsAsync(CRMAnalyticsRequest request);
    Task<CRMWorkflow> CreateWorkflowAsync(CRMWorkflowRequest request);
    Task<CRMSegmentation> SegmentCustomersAsync(SegmentationRequest request);
}

public interface IECommerceIntegrationService
{
    Task<ECommerceConnection> ConnectToECommerceAsync(ECommerceConnectionConfig config);
    Task<ProductSync> SyncProductCatalogAsync(ProductCatalogSyncRequest request);
    Task<OrderSync> SyncECommerceOrdersAsync(ECommerceOrderSyncRequest request);
    Task<CustomerSync> SyncECommerceCustomersAsync(ECommerceCustomerSyncRequest request);
    Task<InventorySync> SyncECommerceInventoryAsync(ECommerceInventorySyncRequest request);
    Task<PricingSync> SyncPricingAsync(PricingSyncRequest request);
    Task<PromotionSync> SyncPromotionsAsync(PromotionSyncRequest request);
    Task<ShippingSync> SyncShippingDataAsync(ShippingSyncRequest request);
    Task<PaymentSync> SyncPaymentDataAsync(PaymentSyncRequest request);
    Task<MarketplaceSync> SyncMarketplaceDataAsync(MarketplaceSyncRequest request);
}