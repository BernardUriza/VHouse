using System.ComponentModel.DataAnnotations;

namespace VHouse.Classes;

// Additional Integration models to resolve compilation errors
public class CRMAnalyticsRequest
{
    public string Platform { get; set; } = string.Empty;
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public List<string> Metrics { get; set; } = new();
}

public class CRMAnalytics
{
    public string AnalyticsId { get; set; } = string.Empty;
    public Dictionary<string, object> Metrics { get; set; } = new();
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

public class CRMWorkflowRequest
{
    public string WorkflowName { get; set; } = string.Empty;
    public string TriggerEvent { get; set; } = string.Empty;
    public Dictionary<string, object> Parameters { get; set; } = new();
}

public class CRMWorkflow
{
    public string WorkflowId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class SegmentationRequest
{
    public string Platform { get; set; } = string.Empty;
    public List<string> Criteria { get; set; } = new();
    public Dictionary<string, object> Parameters { get; set; } = new();
}

public class CRMSegmentation
{
    public string SegmentationId { get; set; } = string.Empty;
    public List<CustomerSegment> Segments { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class CustomerSegment
{
    public string SegmentId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int CustomerCount { get; set; }
    public Dictionary<string, object> Criteria { get; set; } = new();
}

public class ECommerceConnectionConfig
{
    public string ConnectionId { get; set; } = string.Empty;
    public string Platform { get; set; } = string.Empty;
    public string StoreUrl { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public Dictionary<string, string> Settings { get; set; } = new();
}

// Additional missing Integration models for ERP
public class FinancialDataSyncRequest
{
    public string RequestId { get; set; } = string.Empty;
    public string ERPSystem { get; set; } = string.Empty;
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public List<string> DataTypes { get; set; } = new(); // GL, AR, AP, etc.
    public Dictionary<string, object> Filters { get; set; } = new();
}

public class ERPSyncResult
{
    public string SyncId { get; set; } = string.Empty;
    public string RequestId { get; set; } = string.Empty;
    public bool Success { get; set; }
    public int RecordsProcessed { get; set; }
    public int RecordsCreated { get; set; }
    public int RecordsUpdated { get; set; }
    public int RecordsSkipped { get; set; }
    public List<string> Errors { get; set; } = new();
    public TimeSpan ProcessingTime { get; set; }
    public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
}

public class SupplierDataSyncRequest
{
    public string RequestId { get; set; } = string.Empty;
    public string ERPSystem { get; set; } = string.Empty;
    public List<string> SupplierIds { get; set; } = new();
    public bool IncludeContacts { get; set; } = true;
    public bool IncludePaymentTerms { get; set; } = true;
    public Dictionary<string, object> SyncOptions { get; set; } = new();
}

public class PurchaseOrderSyncRequest
{
    public string RequestId { get; set; } = string.Empty;
    public string ERPSystem { get; set; } = string.Empty;
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public List<string> POStatuses { get; set; } = new();
    public List<string> SupplierIds { get; set; } = new();
}

public class AccountingDataSyncRequest
{
    public string RequestId { get; set; } = string.Empty;
    public string ERPSystem { get; set; } = string.Empty;
    public string AccountingPeriod { get; set; } = string.Empty;
    public List<string> AccountCategories { get; set; } = new();
    public bool IncludeJournalEntries { get; set; } = true;
    public Dictionary<string, object> Parameters { get; set; } = new();
}

public class ERPInventoryRequest
{
    public string RequestId { get; set; } = string.Empty;
    public string ERPSystem { get; set; } = string.Empty;
    public List<string> ProductIds { get; set; } = new();
    public List<string> Warehouses { get; set; } = new();
    public bool IncludeReserved { get; set; } = true;
    public bool IncludeOnOrder { get; set; } = true;
}

public class ERPInventory
{
    public string InventoryId { get; set; } = string.Empty;
    public string ProductId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public int QuantityOnHand { get; set; }
    public int QuantityReserved { get; set; }
    public int QuantityOnOrder { get; set; }
    public decimal UnitCost { get; set; }
    public string Warehouse { get; set; } = string.Empty;
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}

public class ERPReportRequest
{
    public string RequestId { get; set; } = string.Empty;
    public string ERPSystem { get; set; } = string.Empty;
    public string ReportType { get; set; } = string.Empty;
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public Dictionary<string, object> Parameters { get; set; } = new();
    public string OutputFormat { get; set; } = "JSON";
}

public class ERPReport
{
    public string ReportId { get; set; } = string.Empty;
    public string ReportType { get; set; } = string.Empty;
    public string ReportName { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public Dictionary<string, object> Data { get; set; } = new();
    public string Format { get; set; } = string.Empty;
    public long Size { get; set; }
}

public class ERPTransactionRequest
{
    public string RequestId { get; set; } = string.Empty;
    public string ERPSystem { get; set; } = string.Empty;
    public string TransactionType { get; set; } = string.Empty;
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public List<string> AccountIds { get; set; } = new();
}

public class ERPTransaction
{
    public string TransactionId { get; set; } = string.Empty;
    public string TransactionType { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public string Description { get; set; } = string.Empty;
    public string AccountId { get; set; } = string.Empty;
    public string Reference { get; set; } = string.Empty;
    public Dictionary<string, object> Details { get; set; } = new();
}

public class ERPReconciliationRequest
{
    public string RequestId { get; set; } = string.Empty;
    public string ERPSystem { get; set; } = string.Empty;
    public string ReconciliationType { get; set; } = string.Empty;
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public List<string> AccountIds { get; set; } = new();
}

public class ERPReconciliation
{
    public string ReconciliationId { get; set; } = string.Empty;
    public string ReconciliationType { get; set; } = string.Empty;
    public DateTime ReconciliationDate { get; set; } = DateTime.UtcNow;
    public bool IsReconciled { get; set; }
    public decimal Variance { get; set; }
    public List<string> Discrepancies { get; set; } = new();
    public Dictionary<string, object> Details { get; set; } = new();
}

// CRM Integration models
public class CRMConnectionConfig
{
    public string ConnectionId { get; set; } = string.Empty;
    public string CRMPlatform { get; set; } = string.Empty;
    public string ServerUrl { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
    public Dictionary<string, string> CustomFields { get; set; } = new();
    public DateTime TokenExpiry { get; set; }
}

public class ContactSyncRequest
{
    public string RequestId { get; set; } = string.Empty;
    public string CRMPlatform { get; set; } = string.Empty;
    public DateTime? LastSyncDate { get; set; }
    public List<string> ContactSegments { get; set; } = new();
    public bool IncludeActivities { get; set; } = true;
}

public class LeadSyncRequest
{
    public string RequestId { get; set; } = string.Empty;
    public string CRMPlatform { get; set; } = string.Empty;
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public List<string> LeadSources { get; set; } = new();
    public List<string> LeadStatuses { get; set; } = new();
}

public class OpportunitySyncRequest
{
    public string RequestId { get; set; } = string.Empty;
    public string CRMPlatform { get; set; } = string.Empty;
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public List<string> PipelineStages { get; set; } = new();
    public decimal MinAmount { get; set; } = 0;
}

public class ActivitySyncRequest
{
    public string RequestId { get; set; } = string.Empty;
    public string CRMPlatform { get; set; } = string.Empty;
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public List<string> ActivityTypes { get; set; } = new();
    public List<string> UserIds { get; set; } = new();
}

public class CampaignRequest
{
    public string RequestId { get; set; } = string.Empty;
    public string CRMPlatform { get; set; } = string.Empty;
    public string CampaignType { get; set; } = string.Empty;
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public List<string> CampaignStatuses { get; set; } = new();
}

public class CRMCampaign
{
    public string CampaignId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal Budget { get; set; }
    public int TargetAudience { get; set; }
    public Dictionary<string, object> Metrics { get; set; } = new();
}

// Additional missing integration models
public class ERPConnection
{
    public string ConnectionId { get; set; } = string.Empty;
    public string ERPSystem { get; set; } = string.Empty;
    public string ServerUrl { get; set; } = string.Empty;
    public string Database { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime LastConnected { get; set; }
    public Dictionary<string, string> ConnectionProperties { get; set; } = new();
}

// General Integration models
public class IntegrationValidationRequest
{
    public string IntegrationType { get; set; } = string.Empty;
    public Dictionary<string, object> Data { get; set; } = new();
    public List<string> ValidationRules { get; set; } = new();
    public string DataSource { get; set; } = string.Empty;
}

public class ValidationResult
{
    public string ValidationId { get; set; } = string.Empty;
    public bool IsValid { get; set; }
    public List<ValidationError> Errors { get; set; } = new();
    public List<ValidationWarning> Warnings { get; set; } = new();
    public DateTime ValidatedAt { get; set; } = DateTime.UtcNow;
    public Dictionary<string, object> ValidationMetadata { get; set; } = new();
}

public class ValidationError
{
    public string ErrorCode { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
    public string Field { get; set; } = string.Empty;
    public string Severity { get; set; } = "Error";
}

public class ValidationWarning
{
    public string WarningCode { get; set; } = string.Empty;
    public string WarningMessage { get; set; } = string.Empty;
    public string Field { get; set; } = string.Empty;
    public string Recommendation { get; set; } = string.Empty;
}