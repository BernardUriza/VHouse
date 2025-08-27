using System.ComponentModel.DataAnnotations;

namespace VHouse.Classes;

// Container Orchestration Models
public class ContainerNetworkConfiguration
{
    public string NetworkId { get; set; } = string.Empty;
    public string NetworkName { get; set; } = string.Empty;
    public string Driver { get; set; } = string.Empty;
    public Dictionary<string, string> Options { get; set; } = new();
    public List<string> Subnets { get; set; } = new();
    public string Gateway { get; set; } = string.Empty;
    public bool EnableIPv6 { get; set; }
    public Dictionary<string, object> Labels { get; set; } = new();
    public bool Internal { get; set; }
    public bool Attachable { get; set; }
    public string Scope { get; set; } = string.Empty;
}

public class NetworkConfiguration
{
    public string NetworkId { get; set; } = string.Empty;
    public string NetworkName { get; set; } = string.Empty;
    public string NetworkType { get; set; } = string.Empty;
    public string Subnet { get; set; } = string.Empty;
    public string Gateway { get; set; } = string.Empty;
    public List<string> DNSServers { get; set; } = new();
    public Dictionary<string, string> NetworkOptions { get; set; } = new();
    public bool EnableIPv6 { get; set; }
}

// Container Orchestration Models
public class ContainerDeploymentConfig
{
    public string ImageName { get; set; } = string.Empty;
    public string Tag { get; set; } = string.Empty;
    public int Replicas { get; set; } = 1;
    public Dictionary<string, string> EnvironmentVariables { get; set; } = new();
    public List<string> Command { get; set; } = new();
    public ResourceLimits Resources { get; set; } = new();
    public Dictionary<string, string> Labels { get; set; } = new();
    public HealthCheckConfiguration HealthCheck { get; set; } = new();
    public string ServiceName { get; set; } = string.Empty;
    public string ImageTag { get; set; } = string.Empty;
}

public class KubernetesDeploymentConfig : ContainerDeploymentConfig
{
    public string Namespace { get; set; } = string.Empty;
    public string DeploymentStrategy { get; set; } = string.Empty;
    public Dictionary<string, string> Annotations { get; set; } = new();
    public List<VolumeMount> VolumeMounts { get; set; } = new();
    public List<ConfigMapReference> ConfigMaps { get; set; } = new();
    public List<SecretReference> Secrets { get; set; } = new();
    public ServiceConfiguration Service { get; set; } = new();
    public IngressConfiguration Ingress { get; set; } = new();
}

public class ContainerDeploymentResult
{
    public string DeploymentId { get; set; } = string.Empty;
    public string ServiceId { get; set; } = string.Empty;
    public bool Success { get; set; }
    public List<string> ContainerIds { get; set; } = new();
    public string Status { get; set; } = string.Empty;
    public DateTime DeployedAt { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
    
    public string ServiceEndpoint { get; set; } = string.Empty;
}

public class ContainerUpdateConfig
{
    public string ImageTag { get; set; } = string.Empty;
    public int? Replicas { get; set; }
    public Dictionary<string, string> EnvironmentVariables { get; set; } = new();
    public ResourceLimits? Resources { get; set; }
    public string UpdateStrategy { get; set; } = string.Empty;
}

public class ServiceMeshRequest
{
    public string ServiceName { get; set; } = string.Empty;
    public string MeshType { get; set; } = string.Empty;
    public Dictionary<string, object> Configuration { get; set; } = new();
}

public class ServiceMeshConfig
{
    public string MeshId { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public bool TrafficManagementEnabled { get; set; }
    public bool SecurityPoliciesEnabled { get; set; }
    public Dictionary<string, object> Settings { get; set; } = new();
}

public class ServiceMeshMetrics
{
    public string MeshId { get; set; } = string.Empty;
    public Dictionary<string, double> TrafficMetrics { get; set; } = new();
    public Dictionary<string, double> SecurityMetrics { get; set; } = new();
    public DateTime CollectedAt { get; set; }
}

public class ServiceMeshPolicy
{
    public string PolicyName { get; set; } = string.Empty;
    public string PolicyType { get; set; } = string.Empty;
    public Dictionary<string, object> Rules { get; set; } = new();
}

public class ContainerImage
{
    public string Name { get; set; } = string.Empty;
    public string Tag { get; set; } = string.Empty;
    public string Registry { get; set; } = string.Empty;
    public long Size { get; set; }
    public DateTime CreatedAt { get; set; }
    public Dictionary<string, string> Labels { get; set; } = new();
}

public class ImagePushResult
{
    public bool Success { get; set; }
    public string ImageId { get; set; } = string.Empty;
    public string Registry { get; set; } = string.Empty;
    public string Tag { get; set; } = string.Empty;
    public DateTime PushedAt { get; set; }
}

public class ContainerServiceHealth
{
    public string ServiceId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int HealthyReplicas { get; set; }
    public int TotalReplicas { get; set; }
    public List<ContainerHealth> ContainerHealths { get; set; } = new();
    public DateTime LastChecked { get; set; }
}

public class ContainerHealth
{
    public string ContainerId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime LastHealthCheck { get; set; }
    public Dictionary<string, object> HealthDetails { get; set; } = new();
}

public class ContainerLogs
{
    public string ContainerId { get; set; } = string.Empty;
    public List<LogEntry> Entries { get; set; } = new();
    public DateTime RetrievedAt { get; set; }
}

public class LogEntry
{
    public DateTime Timestamp { get; set; }
    public string Level { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public Dictionary<string, object> Metadata { get; set; } = new();
    public Dictionary<string, object> Details { get; set; } = new();
}

public class LogQueryOptions
{
    public DateTime? Since { get; set; }
    public DateTime? Until { get; set; }
    public int? Lines { get; set; }
    public bool Follow { get; set; }
    public string LogLevel { get; set; } = string.Empty;
}

public class ContainerNetwork
{
    public string NetworkId { get; set; } = string.Empty;
    public string NetworkName { get; set; } = string.Empty;
    public string Driver { get; set; } = string.Empty;
    public string Subnet { get; set; } = string.Empty;
    public string Gateway { get; set; } = string.Empty;
    public List<string> ConnectedContainers { get; set; } = new();
    public Dictionary<string, object> Configuration { get; set; } = new();
}

public class ContainerService
{
    public string ServiceId { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int Replicas { get; set; }
    public string ImageName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public Dictionary<string, object> Configuration { get; set; } = new();
    
    // Instance properties for compatibility
    public string Name { get; set; } = string.Empty;
    public int DesiredReplicas { get; set; }
    public int RunningReplicas { get; set; }
    public string ImageTag { get; set; } = string.Empty;
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}

// Supporting Classes
public class ResourceLimits
{
    public string Memory { get; set; } = string.Empty;
    public string CPU { get; set; } = string.Empty;
    public string Storage { get; set; } = string.Empty;
}

public class HealthCheckConfiguration
{
    public string Command { get; set; } = string.Empty;
    public int IntervalSeconds { get; set; } = 30;
    public int TimeoutSeconds { get; set; } = 10;
    public int Retries { get; set; } = 3;
}

public class VolumeMount
{
    public string Name { get; set; } = string.Empty;
    public string MountPath { get; set; } = string.Empty;
    public bool ReadOnly { get; set; }
}

public class ConfigMapReference
{
    public string Name { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
}

public class SecretReference
{
    public string Name { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
}

public class ServiceConfiguration
{
    public string Type { get; set; } = string.Empty;
    public List<ServicePort> Ports { get; set; } = new();
}

public class ServicePort
{
    public int Port { get; set; }
    public int TargetPort { get; set; }
    public string Protocol { get; set; } = "TCP";
}

public class IngressConfiguration
{
    public string Host { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public int ServicePort { get; set; }
}

public class HealthCheckResult
{
    public string ResourceId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CheckTime { get; set; }
    public string Message { get; set; } = string.Empty;
    public Dictionary<string, object> Details { get; set; } = new();
    public TimeSpan ResponseTime { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public double HealthScore { get; set; }
    public List<string> Warnings { get; set; } = new();
    public List<string> Errors { get; set; } = new();
}

public class ResourceType
{
    public string TypeId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Dictionary<string, object> Properties { get; set; } = new();
    public List<string> RequiredFields { get; set; } = new();
    public Dictionary<string, string> DefaultValues { get; set; } = new();
    public List<string> SupportedActions { get; set; } = new();
}


// API Gateway Models
public class APIRequest
{
    public string RequestId { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public Dictionary<string, string> Headers { get; set; } = new();
    public Dictionary<string, string> QueryParameters { get; set; } = new();
    public string Body { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string IPAddress { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public Dictionary<string, object> Context { get; set; } = new();
}

public class APIResponse
{
    public string RequestId { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public Dictionary<string, string> Headers { get; set; } = new();
    public string Body { get; set; } = string.Empty;
    public TimeSpan ProcessingTime { get; set; }
    public string TargetService { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public class RoutingConfig
{
    public string RouteId { get; set; } = string.Empty;
    public string PathPattern { get; set; } = string.Empty;
    public List<string> Methods { get; set; } = new();
    public string TargetServiceUrl { get; set; } = string.Empty;
    public Dictionary<string, string> Headers { get; set; } = new();
    public List<string> Middleware { get; set; } = new();
    public LoadBalancingStrategy LoadBalancing { get; set; }
    public TimeSpan Timeout { get; set; }
    public RetryPolicy RetryPolicy { get; set; } = new();
    public bool EnableCaching { get; set; }
    public CachingConfig CachingConfig { get; set; } = new();
}

public class RateLimitPolicy
{
    public string PolicyId { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public int RequestsPerMinute { get; set; }
    public int RequestsPerHour { get; set; }
    public int RequestsPerDay { get; set; }
    public bool EnableBurst { get; set; }
    public int BurstSize { get; set; }
    public TimeSpan WindowSize { get; set; }
    public RateLimitStrategy Strategy { get; set; }
    public List<string> ExemptEndpoints { get; set; } = new();
    public Dictionary<string, object> CustomRules { get; set; } = new();
}

public class RateLimitResult
{
    public bool Allowed { get; set; }
    public int RemainingRequests { get; set; }
    public TimeSpan ResetTime { get; set; }
    public string LimitType { get; set; } = string.Empty;
    public int CurrentUsage { get; set; }
    public int Limit { get; set; }
    public string? ReasonCode { get; set; }
    public Dictionary<string, object> Headers { get; set; } = new();
}

public class APICredentials
{
    public string ApiKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string Scope { get; set; } = string.Empty;
    public string TokenType { get; set; } = "Bearer";
    public DateTime ExpiresAt { get; set; }
    public List<string> Permissions { get; set; } = new();
    public Dictionary<string, object> Claims { get; set; } = new();
}

public class AuthenticationResult
{
    public bool Success { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
    public List<string> Permissions { get; set; } = new();
    public string AccessToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public string? ErrorMessage { get; set; }
    public Dictionary<string, object> UserContext { get; set; } = new();
}

public class APIAnalytics
{
    public string ApiId { get; set; } = string.Empty;
    public DateTime Period { get; set; }
    public int TotalRequests { get; set; }
    public int SuccessfulRequests { get; set; }
    public int FailedRequests { get; set; }
    public TimeSpan AverageResponseTime { get; set; }
    public TimeSpan P95ResponseTime { get; set; }
    public Dictionary<int, int> StatusCodeDistribution { get; set; } = new();
    public Dictionary<string, int> EndpointUsage { get; set; } = new();
    public List<APIUsagePattern> UsagePatterns { get; set; } = new();
    public Dictionary<string, object> CustomMetrics { get; set; } = new();
}

public class APIUsagePattern
{
    public DateTime Hour { get; set; }
    public int RequestCount { get; set; }
    public TimeSpan AverageResponseTime { get; set; }
    public double ErrorRate { get; set; }
}

// Integration Models
public class ERPSyncRequest
{
    public string ERPSystem { get; set; } = string.Empty;
    public string SyncType { get; set; } = string.Empty;
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public List<string> DataTypes { get; set; } = new();
    public Dictionary<string, object> FilterCriteria { get; set; } = new();
    public bool IncludeDeleted { get; set; }
    public int BatchSize { get; set; } = 1000;
    public Dictionary<string, string> MappingRules { get; set; } = new();
    public string DestinationFormat { get; set; } = string.Empty;
}

public class IntegrationResult
{
    public string IntegrationId { get; set; } = string.Empty;
    public bool Success { get; set; }
    public int ProcessedRecords { get; set; }
    public int SuccessfulRecords { get; set; }
    public int FailedRecords { get; set; }
    public TimeSpan ProcessingTime { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public Dictionary<string, object> Statistics { get; set; } = new();
    public string NextSyncToken { get; set; } = string.Empty;
    public DateTime LastSyncTime { get; set; }
}

public class CRMData
{
    public string CRMSystem { get; set; } = string.Empty;
    public List<Contact> Contacts { get; set; } = new();
    public List<Lead> Leads { get; set; } = new();
    public List<Opportunity> Opportunities { get; set; } = new();
    public List<Account> Accounts { get; set; } = new();
    public List<Activity> Activities { get; set; } = new();
    public Dictionary<string, object> CustomFields { get; set; } = new();
    public DateTime SyncTimestamp { get; set; }
    public string SyncBatch { get; set; } = string.Empty;
}

public class CRMSyncResult
{
    public bool Success { get; set; }
    public int ContactsProcessed { get; set; }
    public int LeadsProcessed { get; set; }
    public int OpportunitiesProcessed { get; set; }
    public int AccountsProcessed { get; set; }
    public int ActivitiesProcessed { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<ConflictResolution> Conflicts { get; set; } = new();
    public TimeSpan ProcessingTime { get; set; }
    public DateTime CompletedAt { get; set; }
}

public class Contact
{
    public string Id { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Company { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Dictionary<string, object> CustomFields { get; set; } = new();
}

public class Lead
{
    public string Id { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Company { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public double Score { get; set; }
    public DateTime CreatedAt { get; set; }
    public Dictionary<string, object> CustomFields { get; set; } = new();
}

public class Opportunity
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string AccountId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Stage { get; set; } = string.Empty;
    public double Probability { get; set; }
    public DateTime CloseDate { get; set; }
    public string Owner { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public Dictionary<string, object> CustomFields { get; set; } = new();
}

public class Account
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Industry { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public decimal AnnualRevenue { get; set; }
    public int EmployeeCount { get; set; }
    public string Owner { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public Dictionary<string, object> CustomFields { get; set; } = new();
}

public class Activity
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ContactId { get; set; } = string.Empty;
    public string AccountId { get; set; } = string.Empty;
    public DateTime ScheduledAt { get; set; }
    public DateTime CompletedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Owner { get; set; } = string.Empty;
}

public class WebhookPayload
{
    public string WebhookId { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public Dictionary<string, object> Data { get; set; } = new();
    public string Signature { get; set; } = string.Empty;
    public string DeliveryId { get; set; } = string.Empty;
    public int RetryCount { get; set; }
    public Dictionary<string, string> Headers { get; set; } = new();
}

public class WebhookResult
{
    public bool Success { get; set; }
    public string ProcessingId { get; set; } = string.Empty;
    public TimeSpan ProcessingTime { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public List<string> ActionsPerformed { get; set; } = new();
    public Dictionary<string, object> ProcessingResults { get; set; } = new();
    public DateTime ProcessedAt { get; set; }
}

public class ConnectorHealth
{
    public string ConnectorId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool IsHealthy { get; set; }
    public DateTime LastHealthCheck { get; set; }
    public TimeSpan ResponseTime { get; set; }
    public int ErrorCount { get; set; }
    public string? LastError { get; set; }
    public Dictionary<string, object> HealthMetrics { get; set; } = new();
    public List<HealthCheckResult> HealthChecks { get; set; } = new();
}


// Blockchain Models
public class SupplyChainEvent
{
    public string EventId { get; set; } = string.Empty;
    public string ProductId { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string Location { get; set; } = string.Empty;
    public string ParticipantId { get; set; } = string.Empty;
    public Dictionary<string, object> EventData { get; set; } = new();
    public List<string> PreviousEvents { get; set; } = new();
    public string TransactionHash { get; set; } = string.Empty;
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public class TransactionResult
{
    public string TransactionHash { get; set; } = string.Empty;
    public string BlockHash { get; set; } = string.Empty;
    public long BlockNumber { get; set; }
    public bool Success { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal GasUsed { get; set; }
    public decimal GasPrice { get; set; }
    public DateTime Timestamp { get; set; }
    public string? ErrorMessage { get; set; }
    public Dictionary<string, object> Logs { get; set; } = new();
    public Dictionary<string, object> Events { get; set; } = new();
}

public class ContractExecution
{
    public string ContractAddress { get; set; } = string.Empty;
    public string FunctionName { get; set; } = string.Empty;
    public List<object> Parameters { get; set; } = new();
    public string CallerAddress { get; set; } = string.Empty;
    public decimal GasLimit { get; set; }
    public decimal GasPrice { get; set; }
    public decimal Value { get; set; }
    public Dictionary<string, object> Context { get; set; } = new();
    public bool DryRun { get; set; }
}

public class SmartContractResult
{
    public string TransactionHash { get; set; } = string.Empty;
    public bool Success { get; set; }
    public object? ReturnValue { get; set; }
    public decimal GasUsed { get; set; }
    public List<ContractEvent> Events { get; set; } = new();
    public string? ErrorMessage { get; set; }
    public Dictionary<string, object> Logs { get; set; } = new();
    public DateTime ExecutedAt { get; set; }
}

public class ContractEvent
{
    public string EventName { get; set; } = string.Empty;
    public Dictionary<string, object> Parameters { get; set; } = new();
    public string TransactionHash { get; set; } = string.Empty;
    public long BlockNumber { get; set; }
    public DateTime Timestamp { get; set; }
}

public class VerificationResult
{
    public bool IsValid { get; set; }
    public string ProductId { get; set; } = string.Empty;
    public string VerificationMethod { get; set; } = string.Empty;
    public DateTime VerifiedAt { get; set; }
    public List<string> VerificationSteps { get; set; } = new();
    public Dictionary<string, object> VerificationData { get; set; } = new();
    public double TrustScore { get; set; }
    public string? VerificationCertificate { get; set; }
    public List<string> Warnings { get; set; } = new();
}

// Supporting Enums
public enum LoadBalancingStrategy
{
    RoundRobin,
    WeightedRoundRobin,
    LeastConnections,
    Random,
    IPHash
}

public enum RateLimitStrategy
{
    FixedWindow,
    SlidingWindow,
    TokenBucket,
    LeakyBucket
}

// Supporting Classes
public class RetryPolicy
{
    public int MaxRetries { get; set; } = 3;
    public TimeSpan InitialDelay { get; set; } = TimeSpan.FromSeconds(1);
    public TimeSpan MaxDelay { get; set; } = TimeSpan.FromSeconds(30);
    public double BackoffMultiplier { get; set; } = 2.0;
    public List<int> RetryableStatusCodes { get; set; } = new() { 429, 502, 503, 504 };
}

public class CachingConfig
{
    public TimeSpan TTL { get; set; } = TimeSpan.FromMinutes(5);
    public bool VaryByHeaders { get; set; }
    public List<string> VaryByHeaderNames { get; set; } = new();
    public bool VaryByQuery { get; set; }
    public List<string> CacheableStatusCodes { get; set; } = new() { "200", "301", "404" };
}

public class ConflictResolution
{
    public string ConflictId { get; set; } = string.Empty;
    public string ConflictType { get; set; } = string.Empty;
    public string Field { get; set; } = string.Empty;
    public object SourceValue { get; set; } = new();
    public object TargetValue { get; set; } = new();
    public string Resolution { get; set; } = string.Empty;
    public DateTime ResolvedAt { get; set; }
}

// Additional API Gateway Models
public class ThrottlingPolicy
{
    public string PolicyId { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public int MaxConcurrentRequests { get; set; }
    public TimeSpan BackoffPeriod { get; set; }
    public string ThrottleStrategy { get; set; } = string.Empty;
    public Dictionary<string, object> CustomRules { get; set; } = new();
}

public class ThrottlingResult
{
    public string ClientId { get; set; } = string.Empty;
    public bool ThrottleApplied { get; set; }
    public int DelayMs { get; set; }
    public string Reason { get; set; } = string.Empty;
    public TimeSpan RetryAfter { get; set; }
}

public class CacheRequest
{
    public string Key { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public TimeSpan TTL { get; set; }
    public object? Data { get; set; }
}

public class CachingResult
{
    public bool Success { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public TimeSpan TTL { get; set; }
    public double HitRate { get; set; }
    public long Size { get; set; }
}

public class LoadBalancingRequest
{
    public string ServiceName { get; set; } = string.Empty;
    public List<string> Endpoints { get; set; } = new();
    public LoadBalancingStrategy Strategy { get; set; }
    public Dictionary<string, double> Weights { get; set; } = new();
}

public class LoadBalancingResult
{
    public string SelectedEndpoint { get; set; } = string.Empty;
    public LoadBalancingStrategy Strategy { get; set; }
    public Dictionary<string, double> LoadDistribution { get; set; } = new();
    public int HealthyEndpoints { get; set; }
    public int TotalEndpoints { get; set; }
}

public class APIKeyRequest
{
    public string ClientId { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public List<string> Permissions { get; set; } = new();
    public DateTime ExpiresAt { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public class APIKey
{
    public string KeyId { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public List<string> Permissions { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsActive { get; set; }
}


public class TransformationRequest
{
    public string RequestId { get; set; } = string.Empty;
    public object OriginalPayload { get; set; } = new();
    public List<object> TransformationRules { get; set; } = new();
    public string TargetFormat { get; set; } = string.Empty;
}

public class TransformationResult
{
    public bool Success { get; set; }
    public string RequestId { get; set; } = string.Empty;
    public object TransformedPayload { get; set; } = new();
    public int TransformationsApplied { get; set; }
    public TimeSpan ProcessingTime { get; set; }
}

public class MonitoringRequest
{
    public string ApiId { get; set; } = string.Empty;
    public List<string> Metrics { get; set; } = new();
    public TimeSpan MonitoringPeriod { get; set; }
}

public class MonitoringResult
{
    public string ApiId { get; set; } = string.Empty;
    public bool IsHealthy { get; set; }
    public TimeSpan ResponseTime { get; set; }
    public TimeSpan Uptime { get; set; }
    public double ErrorRate { get; set; }
    public int ThroughputRPS { get; set; }
    public DateTime LastChecked { get; set; }
    public double HealthScore { get; set; }
}

public class QuotaRequest
{
    public string ClientId { get; set; } = string.Empty;
    public int Limit { get; set; }
    public TimeSpan Period { get; set; }
    public string QuotaType { get; set; } = string.Empty;
}

public class QuotaResult
{
    public string ClientId { get; set; } = string.Empty;
    public int Limit { get; set; }
    public int Used { get; set; }
    public int Remaining { get; set; }
    public DateTime ResetDate { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class SecurityRequest
{
    public string RequestId { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public Dictionary<string, string> Headers { get; set; } = new();
    public string Payload { get; set; } = string.Empty;
    public string IPAddress { get; set; } = string.Empty;
}

public class SecurityResult
{
    public string RequestId { get; set; } = string.Empty;
    public bool ThreatDetected { get; set; }
    public string? ThreatType { get; set; }
    public double RiskScore { get; set; }
    public string Action { get; set; } = string.Empty;
    public List<string> SecurityChecks { get; set; } = new();
    public TimeSpan ProcessingTime { get; set; }
}

public class DocumentationRequest
{
    public string ApiId { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Format { get; set; } = "OpenAPI";
    public bool IncludeExamples { get; set; } = true;
    public List<string> Sections { get; set; } = new();
}

public class DocumentationResult
{
    public string ApiId { get; set; } = string.Empty;
    public string DocumentationUrl { get; set; } = string.Empty;
    public string Format { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }
    public string Version { get; set; } = string.Empty;
    public long Size { get; set; }
    public List<string> Sections { get; set; } = new();
}

// Additional Integration Models
public class ECommerceSyncRequest
{
    public string Platform { get; set; } = string.Empty;
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public List<string> DataTypes { get; set; } = new();
    public Dictionary<string, object> Configuration { get; set; } = new();
}

public class ECommerceSync
{
    public string SyncId { get; set; } = string.Empty;
    public string Platform { get; set; } = string.Empty;
    public bool Success { get; set; }
    public int ProductsSynced { get; set; }
    public int OrdersSynced { get; set; }
    public int CustomersSynced { get; set; }
    public int InventorySynced { get; set; }
    public TimeSpan ProcessingTime { get; set; }
    public DateTime LastSyncTime { get; set; }
    public DateTime NextScheduledSync { get; set; }
    public List<string> Errors { get; set; } = new();
    public Dictionary<string, object> SyncMetrics { get; set; } = new();
}

public class InventorySyncRequest
{
    public string SyncType { get; set; } = string.Empty;
    public List<IntegrationInventoryItem> InventoryItems { get; set; } = new();
    public string Source { get; set; } = string.Empty;
    public bool ValidateQuantities { get; set; } = true;
}

public class IntegrationInventoryItem
{
    public string ProductId { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public string Location { get; set; } = string.Empty;
    public DateTime LastUpdated { get; set; }
}

public class InventorySync
{
    public string SyncId { get; set; } = string.Empty;
    public bool Success { get; set; }
    public int ItemsProcessed { get; set; }
    public int ItemsUpdated { get; set; }
    public int ConflictsDetected { get; set; }
    public TimeSpan ProcessingTime { get; set; }
    public string SyncType { get; set; } = string.Empty;
    public List<InventoryConflict> Conflicts { get; set; } = new();
    public DateTime CompletedAt { get; set; }
}

public class InventoryConflict
{
    public string ProductId { get; set; } = string.Empty;
    public string ConflictType { get; set; } = string.Empty;
    public int SourceQuantity { get; set; }
    public int TargetQuantity { get; set; }
    public string RecommendedAction { get; set; } = string.Empty;
}

public class CustomerSyncRequest
{
    public string Source { get; set; } = string.Empty;
    public List<string> CustomerIds { get; set; } = new();
    public DateTime? LastSyncTime { get; set; }
    public Dictionary<string, object> Filters { get; set; } = new();
}

public class CustomerSync
{
    public string SyncId { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public bool Success { get; set; }
    public int CustomersProcessed { get; set; }
    public int CustomersCreated { get; set; }
    public int CustomersUpdated { get; set; }
    public int DuplicatesFound { get; set; }
    public TimeSpan ProcessingTime { get; set; }
    public DateTime CompletedAt { get; set; }
}

public class OrderSyncRequest
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public List<string> OrderStatuses { get; set; } = new();
    public string Source { get; set; } = string.Empty;
    public bool IncludePayments { get; set; } = true;
    public bool IncludeShipping { get; set; } = true;
}

public class OrderSync
{
    public string SyncId { get; set; } = string.Empty;
    public bool Success { get; set; }
    public int OrdersProcessed { get; set; }
    public int OrdersCreated { get; set; }
    public int OrdersUpdated { get; set; }
    public int PaymentsSynced { get; set; }
    public int ShippingUpdated { get; set; }
    public TimeSpan ProcessingTime { get; set; }
    public object DateRange { get; set; } = new();
    public DateTime CompletedAt { get; set; }
}

public class IntegrationStatus
{
    public string IntegrationId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime LastSyncTime { get; set; }
    public DateTime NextScheduledSync { get; set; }
    public double HealthScore { get; set; }
    public int ErrorCount { get; set; }
    public double SuccessRate { get; set; }
    public Dictionary<string, object> Configuration { get; set; } = new();
}

public class DataMappingRequest
{
    public string Name { get; set; } = string.Empty;
    public string SourceSchema { get; set; } = string.Empty;
    public string TargetSchema { get; set; } = string.Empty;
    public Dictionary<string, string> FieldMappings { get; set; } = new();
    public List<object> TransformationRules { get; set; } = new();
    public List<object> ValidationRules { get; set; } = new();
    public string MappingType { get; set; } = string.Empty;
}

public class DataMapping
{
    public string MappingId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string SourceSchema { get; set; } = string.Empty;
    public string TargetSchema { get; set; } = string.Empty;
    public Dictionary<string, string> FieldMappings { get; set; } = new();
    public List<object> TransformationRules { get; set; } = new();
    public List<object> ValidationRules { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsActive { get; set; }
    public string MappingType { get; set; } = string.Empty;
}

public class DataTransformationRequest
{
    public string MappingId { get; set; } = string.Empty;
    public object SourceData { get; set; } = new();
    public List<object> TransformationRules { get; set; } = new();
    public string TargetFormat { get; set; } = string.Empty;
}


public class SyncScheduleRequest
{
    public string IntegrationId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string CronExpression { get; set; } = string.Empty;
    public string TimeZone { get; set; } = "UTC";
    public Dictionary<string, object> Configuration { get; set; } = new();
}

public class SyncSchedule
{
    public string ScheduleId { get; set; } = string.Empty;
    public string IntegrationId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string CronExpression { get; set; } = string.Empty;
    public string TimeZone { get; set; } = string.Empty;
    public DateTime NextRun { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public Dictionary<string, object> Configuration { get; set; } = new();
}

public class LogFilter
{
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public List<string> LogLevels { get; set; } = new();
    public string? SearchTerm { get; set; }
    public int Limit { get; set; } = 100;
    public int Offset { get; set; } = 0;
}

public class IntegrationLog
{
    public string IntegrationId { get; set; } = string.Empty;
    public LogFilter Filter { get; set; } = new();
    public int TotalEntries { get; set; }
    public List<LogEntry> Entries { get; set; } = new();
    public bool HasMore { get; set; }
    public DateTime GeneratedAt { get; set; }
}


public class ConflictResolutionRequest
{
    public string ConflictId { get; set; } = string.Empty;
    public string ConflictType { get; set; } = string.Empty;
    public string Field { get; set; } = string.Empty;
    public object SourceValue { get; set; } = new();
    public object TargetValue { get; set; } = new();
    public string Resolution { get; set; } = string.Empty;
}

// Additional API Gateway & Partner Models
public class APIDefinition
{
    public string ApiId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> Endpoints { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
}

public class APIDefinitionRequest
{
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> Endpoints { get; set; } = new();
    public Dictionary<string, object> Schema { get; set; } = new();
}

public class APIDefinitionUpdateRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> Endpoints { get; set; } = new();
    public Dictionary<string, object> Schema { get; set; } = new();
}

public class APIListRequest
{
    public string? Filter { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SortBy { get; set; }
}

public class APIDeployment
{
    public string DeploymentId { get; set; } = string.Empty;
    public string ApiId { get; set; } = string.Empty;
    public string Environment { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime DeployedAt { get; set; }
    public string DeployedBy { get; set; } = string.Empty;
}

public class APIDeploymentRequest
{
    public string ApiId { get; set; } = string.Empty;
    public string Environment { get; set; } = string.Empty;
    public Dictionary<string, object> Configuration { get; set; } = new();
}

public class APIStatus
{
    public string ApiId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool IsHealthy { get; set; }
    public DateTime LastChecked { get; set; }
    public TimeSpan Uptime { get; set; }
    public int RequestCount { get; set; }
    public double ErrorRate { get; set; }
}

public class APIMetrics
{
    public string ApiId { get; set; } = string.Empty;
    public int TotalRequests { get; set; }
    public int SuccessfulRequests { get; set; }
    public int FailedRequests { get; set; }
    public TimeSpan AverageResponseTime { get; set; }
    public Dictionary<string, int> EndpointUsage { get; set; } = new();
    public DateTime Period { get; set; }
}

public class MetricsRequest
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public List<string> Metrics { get; set; } = new();
}

public class APILog
{
    public string ApiId { get; set; } = string.Empty;
    public List<LogEntry> Entries { get; set; } = new();
    public int TotalEntries { get; set; }
    public DateTime GeneratedAt { get; set; }
}

public class LogRequest
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public string? LogLevel { get; set; }
    public int Limit { get; set; } = 100;
}

public class EndpointTestRequest
{
    public string ApiId { get; set; } = string.Empty;
    public string Endpoint { get; set; } = string.Empty;
    public string Method { get; set; } = "GET";
    public Dictionary<string, string> Headers { get; set; } = new();
    public string? Body { get; set; }
}

public class TestResult
{
    public bool Success { get; set; }
    public int StatusCode { get; set; }
    public string Response { get; set; } = string.Empty;
    public TimeSpan ResponseTime { get; set; }
    public string? ErrorMessage { get; set; }
}

public class Developer
{
    public string DeveloperId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Company { get; set; } = string.Empty;
    public DateTime RegisteredAt { get; set; }
    public bool IsActive { get; set; }
    public List<string> Applications { get; set; } = new();
}

public class DeveloperRegistration
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Company { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class Application
{
    public string ApplicationId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string DeveloperId { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> ApiKeys { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
}

public class ApplicationRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> RequestedPermissions { get; set; } = new();
}

public class SDK
{
    public string SdkId { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string DownloadUrl { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }
    public long Size { get; set; }
    public string Documentation { get; set; } = string.Empty;
}

public class SDKRequest
{
    public string Language { get; set; } = string.Empty;
    public List<string> ApiIds { get; set; } = new();
    public string Version { get; set; } = string.Empty;
}

public class PartnerPortal
{
    public string PartnerId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public List<Application> Applications { get; set; } = new();
    public List<APIKey> ApiKeys { get; set; } = new();
    public Dictionary<string, object> Usage { get; set; } = new();
}

public class PartnershipRequest
{
    public string RequestId { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public string PartnershipType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class PartnershipApplication
{
    public string CompanyName { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public string ContactName { get; set; } = string.Empty;
    public string PartnershipType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Dictionary<string, object> CompanyInfo { get; set; } = new();
}

public class IntegrationGuide
{
    public string GuideId { get; set; } = string.Empty;
    public string ApiId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public List<string> Examples { get; set; } = new();
    public DateTime LastUpdated { get; set; }
}

public class DeveloperMetrics
{
    public string DeveloperId { get; set; } = string.Empty;
    public int TotalApplications { get; set; }
    public int ActiveApplications { get; set; }
    public int TotalApiCalls { get; set; }
    public Dictionary<string, int> ApiUsage { get; set; } = new();
    public DateTime LastActivity { get; set; }
}

public class Webhook
{
    public string WebhookId { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public List<string> Events { get; set; } = new();
    public string Secret { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class WebhookRegistration
{
    public string Url { get; set; } = string.Empty;
    public List<string> Events { get; set; } = new();
    public string? Secret { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public class SandboxEnvironment
{
    public string SandboxId { get; set; } = string.Empty;
    public string DeveloperId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public Dictionary<string, object> Configuration { get; set; } = new();
}

public class SandboxRequest
{
    public string Name { get; set; } = string.Empty;
    public List<string> ApiIds { get; set; } = new();
    public TimeSpan Duration { get; set; }
    public Dictionary<string, object> Configuration { get; set; } = new();
}

public class Certification
{
    public string CertificationId { get; set; } = string.Empty;
    public string PartnerId { get; set; } = string.Empty;
    public string CertificationType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime IssuedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public Dictionary<string, object> Requirements { get; set; } = new();
}

public class CertificationRequest
{
    public string PartnerId { get; set; } = string.Empty;
    public string CertificationType { get; set; } = string.Empty;
    public Dictionary<string, object> Evidence { get; set; } = new();
}

// Additional missing model classes for E-commerce Integration
public class ProductSync
{
    public int ProductsSynced { get; set; }
    public int ProductsCreated { get; set; }
    public int ProductsUpdated { get; set; }
    public List<string> Errors { get; set; } = new();
}

public class ProductCatalogSyncRequest
{
    public string Platform { get; set; } = string.Empty;
    public DateTime? LastSync { get; set; }
    public List<string> Categories { get; set; } = new();
}

public class ECommerceOrderSyncRequest
{
    public string Platform { get; set; } = string.Empty;
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public List<string> OrderStatuses { get; set; } = new();
}

public class ECommerceCustomerSyncRequest
{
    public string Platform { get; set; } = string.Empty;
    public DateTime? LastSync { get; set; }
    public bool IncludeGuests { get; set; } = false;
}

public class ECommerceInventorySyncRequest
{
    public string Platform { get; set; } = string.Empty;
    public List<string> ProductIds { get; set; } = new();
    public bool SyncQuantityOnly { get; set; } = false;
}

public class PricingSyncRequest
{
    public string Platform { get; set; } = string.Empty;
    public List<string> ProductIds { get; set; } = new();
    public string PricingTier { get; set; } = string.Empty;
}

public class PricingSync
{
    public int PricesUpdated { get; set; }
    public List<string> Errors { get; set; } = new();
}

public class PromotionSyncRequest
{
    public string Platform { get; set; } = string.Empty;
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
}

public class PromotionSync
{
    public int PromotionsSynced { get; set; }
    public List<string> ActivePromotions { get; set; } = new();
}

public class ShippingSyncRequest
{
    public string Platform { get; set; } = string.Empty;
    public List<string> OrderIds { get; set; } = new();
}

public class ShippingSync
{
    public int ShippingUpdated { get; set; }
    public List<string> TrackingNumbers { get; set; } = new();
}

public class PaymentSyncRequest
{
    public string Platform { get; set; } = string.Empty;
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
}

public class PaymentSync
{
    public int PaymentsSynced { get; set; }
    public decimal TotalAmount { get; set; }
}

public class MarketplaceSyncRequest
{
    public string Marketplace { get; set; } = string.Empty;
    public List<string> DataTypes { get; set; } = new();
}

public class MarketplaceSync
{
    public string Marketplace { get; set; } = string.Empty;
    public int ItemsSynced { get; set; }
    public Dictionary<string, object> SyncResults { get; set; } = new();
}

// Cloud Orchestration Models
public class DeploymentResult
{
    public string DeploymentId { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public List<string> Errors { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
    
    public List<string> Resources { get; set; } = new();
    public DateTime EndTime { get; set; }
    public string Provider { get; set; } = string.Empty;
}

public class MultiCloudConfig
{
    public string ConfigId { get; set; } = string.Empty;
    public List<CloudDeployment> Deployments { get; set; } = new();
    public string Strategy { get; set; } = string.Empty;
    public Dictionary<string, object> GlobalSettings { get; set; } = new();
    public List<CloudDeployment> Targets { get; set; } = new();
}

public class CloudDeployment
{
    public string Provider { get; set; } = string.Empty;
    public DeploymentConfig Config { get; set; } = new();
    public int Priority { get; set; } = 1;
    public Dictionary<string, object> ProviderSpecific { get; set; } = new();
}

public class DeploymentStatus
{
    public string DeploymentId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int Progress { get; set; }
    public string CurrentStep { get; set; } = string.Empty;
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    public List<string> CompletedSteps { get; set; } = new();
    
    public static string RollingBack { get; set; } = "RollingBack";
    public static string RolledBack { get; set; } = "RolledBack";
    public static string Failed { get; set; } = "Failed";
}

public enum ScalingDirection
{
    Up,
    Down,
    None,
    Stable
}

public class ScalingResult
{
    public string ResourceId { get; set; } = string.Empty;
    public bool Success { get; set; }
    public int OldInstanceCount { get; set; }
    public int NewInstanceCount { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime ScaledAt { get; set; } = DateTime.UtcNow;
    
    public ScalingDirection Direction { get; set; }
    public int NewCapacity { get; set; }
    public int PreviousCapacity { get; set; }
}

public class ScalingPolicy
{
    public string PolicyId { get; set; } = string.Empty;
    public string PolicyName { get; set; } = string.Empty;
    public int MinInstances { get; set; } = 1;
    public int MaxInstances { get; set; } = 10;
    public double TargetCPUUtilization { get; set; } = 70.0;
    public TimeSpan CooldownPeriod { get; set; } = TimeSpan.FromMinutes(5);
    public Dictionary<string, object> Triggers { get; set; } = new();
    public double ScaleUpThreshold { get; set; } = 80.0;
    public double ScaleDownThreshold { get; set; } = 20.0;
    public string ResourceId { get; set; } = string.Empty;
}

public class ScalingMetrics
{
    public string ResourceId { get; set; } = string.Empty;
    public double CurrentCPUUtilization { get; set; }
    public double CurrentMemoryUtilization { get; set; }
    public int CurrentInstanceCount { get; set; }
    public DateTime LastScalingEvent { get; set; }
    public List<MetricDataPoint> HistoricalMetrics { get; set; } = new();
    public double CpuUtilization { get; set; }
    public double MemoryUtilization { get; set; }
}

public class MetricDataPoint
{
    public DateTime Timestamp { get; set; }
    public string MetricName { get; set; } = string.Empty;
    public double Value { get; set; }
    public string Unit { get; set; } = string.Empty;
}

public class ScalingEvent
{
    public string EventId { get; set; } = string.Empty;
    public string ResourceId { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public int OldCount { get; set; }
    public int NewCount { get; set; }
    public string Trigger { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    public ScalingDirection Direction { get; set; }
    public int InstanceChange { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class FailoverResult
{
    public string FailoverId { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string PrimaryResource { get; set; } = string.Empty;
    public string BackupResource { get; set; } = string.Empty;
    public TimeSpan FailoverTime { get; set; }
    public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;
    public List<string> AffectedServices { get; set; } = new();
}

public class FailoverStrategy
{
    public string StrategyId { get; set; } = string.Empty;
    public string StrategyName { get; set; } = string.Empty;
    public string PrimaryResourceId { get; set; } = string.Empty;
    public List<string> BackupResourceIds { get; set; } = new();
    public TimeSpan MaxFailoverTime { get; set; } = TimeSpan.FromMinutes(5);
    public Dictionary<string, object> Conditions { get; set; } = new();
}

public class DisasterRecoveryPlan
{
    public string PlanId { get; set; } = string.Empty;
    public string PlanName { get; set; } = string.Empty;
    public List<RecoveryStep> RecoverySteps { get; set; } = new();
    public TimeSpan EstimatedRecoveryTime { get; set; }
    public Dictionary<string, object> Resources { get; set; } = new();
    public string Priority { get; set; } = "High";
}

public class RecoveryStep
{
    public string StepId { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Order { get; set; }
    public TimeSpan EstimatedDuration { get; set; }
    public List<string> Dependencies { get; set; } = new();
    public Dictionary<string, object> Parameters { get; set; } = new();
    public string Action { get; set; } = string.Empty;
}

public class FailoverTarget
{
    public string TargetId { get; set; } = string.Empty;
    public string TargetName { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public double Capacity { get; set; }
    public string Status { get; set; } = string.Empty;
    public Dictionary<string, object> Capabilities { get; set; } = new();
}

public class CostOptimization
{
    public string OptimizationId { get; set; } = string.Empty;
    public decimal CurrentMonthlyCost { get; set; }
    public decimal EstimatedMonthlySavings { get; set; }
    public List<CostRecommendation> Recommendations { get; set; } = new();
    public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;
}

public class CostAnalysis
{
    public string AnalysisId { get; set; } = string.Empty;
    public decimal TotalCost { get; set; }
    public Dictionary<string, decimal> CostByService { get; set; } = new();
    public Dictionary<string, decimal> CostByProvider { get; set; } = new();
    public TimeSpan AnalysisPeriod { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

public class CostAnalysisRequest
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public List<string> Services { get; set; } = new();
    public List<string> Providers { get; set; } = new();
    public string Granularity { get; set; } = "daily";
}

public class CostRecommendation
{
    public string RecommendationId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal EstimatedMonthlySavings { get; set; }
    public string Impact { get; set; } = string.Empty;
    public string Effort { get; set; } = string.Empty;
    public Dictionary<string, object> Implementation { get; set; } = new();
    public string Type { get; set; } = string.Empty;
    public string ResourceId { get; set; } = string.Empty;
    public decimal PotentialSavings { get; set; }
    public Dictionary<string, object> Parameters { get; set; } = new();
}

public class CloudResource
{
    public string ResourceId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public Dictionary<string, object> Properties { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public decimal MonthlyCost { get; set; }
    
    public decimal Cost { get; set; }
    public Dictionary<string, string> Tags { get; set; } = new();
}

public class ResourceUtilization
{
    public string ResourceId { get; set; } = string.Empty;
    public double CPUUtilization { get; set; }
    public double MemoryUtilization { get; set; }
    public double StorageUtilization { get; set; }
    public double NetworkUtilization { get; set; }
    public DateTime MeasuredAt { get; set; } = DateTime.UtcNow;
    public Dictionary<string, double> CustomMetrics { get; set; } = new();
}

public class LoadBalancerConfig
{
    public string LoadBalancerId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public List<string> TargetInstances { get; set; } = new();
    public List<HealthCheckConfig> HealthChecks { get; set; } = new();
    public Dictionary<string, object> Rules { get; set; } = new();
    public bool StickySession { get; set; } = false;
}

public class LoadBalancerRequest
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public List<string> Subnets { get; set; } = new();
    public List<string> SecurityGroups { get; set; } = new();
    public LoadBalancerConfig Configuration { get; set; } = new();
}

public class LoadBalancerHealth
{
    public string LoadBalancerId { get; set; } = string.Empty;
    public string OverallStatus { get; set; } = string.Empty;
    public List<TargetHealth> TargetHealths { get; set; } = new();
    public DateTime LastChecked { get; set; } = DateTime.UtcNow;
}

public class TargetHealth
{
    public string TargetId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public DateTime LastStatusChange { get; set; }
}

public class LoadBalancerMetrics
{
    public string LoadBalancerId { get; set; } = string.Empty;
    public int ActiveConnections { get; set; }
    public int RequestsPerSecond { get; set; }
    public TimeSpan AverageResponseTime { get; set; }
    public double ErrorRate { get; set; }
    public DateTime CollectedAt { get; set; } = DateTime.UtcNow;
}

public class HealthCheckConfig
{
    public string Protocol { get; set; } = "HTTP";
    public int Port { get; set; } = 80;
    public string Path { get; set; } = "/health";
    public int IntervalSeconds { get; set; } = 30;
    public int TimeoutSeconds { get; set; } = 5;
    public int HealthyThreshold { get; set; } = 2;
    public int UnhealthyThreshold { get; set; } = 3;
}