using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace VHouse.Interfaces
{
    public interface ICloudOrchestrationService
    {
        // Multi-Cloud Deployment
        Task<DeploymentResult> DeployToCloudAsync(CloudProvider provider, DeploymentConfig config);
        Task<DeploymentResult> DeployMultiCloudAsync(MultiCloudConfig config);
        Task<DeploymentStatus> GetDeploymentStatusAsync(string deploymentId);
        Task<bool> RollbackDeploymentAsync(string deploymentId);
        
        // Auto-Scaling
        Task<ScalingResult> AutoScaleResourcesAsync(ScalingPolicy policy);
        Task<ScalingMetrics> GetScalingMetricsAsync(string resourceId);
        Task<bool> SetAutoScalingPolicyAsync(string resourceId, ScalingPolicy policy);
        Task<List<ScalingEvent>> GetScalingHistoryAsync(string resourceId, DateTime from, DateTime to);
        
        // Failover & Disaster Recovery
        Task<FailoverResult> ExecuteFailoverAsync(FailoverStrategy strategy);
        Task<FailoverResult> ExecuteDisasterRecoveryAsync(DisasterRecoveryPlan plan);
        Task<HealthCheckResult> PerformHealthCheckAsync(string resourceId);
        Task<List<FailoverTarget>> GetAvailableFailoverTargetsAsync(string primaryResourceId);
        
        // Cost Optimization
        Task<CostOptimization> OptimizeCloudCostsAsync();
        Task<CostAnalysis> AnalyzeCloudCostsAsync(CostAnalysisRequest request);
        Task<List<CostRecommendation>> GetCostRecommendationsAsync();
        Task<bool> ApplyCostOptimizationAsync(string recommendationId);
        
        // Resource Management
        Task<List<CloudResource>> GetAllResourcesAsync();
        Task<List<CloudResource>> GetResourcesByProviderAsync(CloudProvider provider);
        Task<ResourceUtilization> GetResourceUtilizationAsync(string resourceId);
        Task<bool> TerminateResourceAsync(string resourceId);
        
        // Load Balancing
        Task<LoadBalancerConfig> CreateLoadBalancerAsync(LoadBalancerRequest request);
        Task<LoadBalancerHealth> GetLoadBalancerHealthAsync(string loadBalancerId);
        Task<bool> UpdateLoadBalancerConfigAsync(string loadBalancerId, LoadBalancerConfig config);
        Task<LoadBalancerMetrics> GetLoadBalancerMetricsAsync(string loadBalancerId);
    }

    // Enums
    public enum CloudProvider
    {
        AWS,
        Azure,
        GCP,
        DigitalOcean,
        OnPremise
    }

    public enum DeploymentStatus
    {
        Pending,
        InProgress,
        Completed,
        Failed,
        Cancelled,
        RollingBack,
        RolledBack
    }

    public enum ScalingDirection
    {
        Up,
        Down,
        Stable
    }

    public enum ResourceType
    {
        ComputeInstance,
        Database,
        LoadBalancer,
        Storage,
        Network,
        Container,
        Function
    }

    // Core Models
    public class DeploymentConfig
    {
        public string ApplicationName { get; set; }
        public string Version { get; set; }
        public string Environment { get; set; }
        public Dictionary<string, string> EnvironmentVariables { get; set; }
        public ResourceRequirements Resources { get; set; }
        public NetworkConfiguration Network { get; set; }
        public SecurityConfiguration Security { get; set; }
        public List<string> Tags { get; set; }
    }

    public class MultiCloudConfig
    {
        public List<CloudDeploymentTarget> Targets { get; set; }
        public LoadDistributionStrategy Distribution { get; set; }
        public FailoverConfiguration Failover { get; set; }
        public DataReplicationStrategy DataReplication { get; set; }
    }

    public class CloudDeploymentTarget
    {
        public CloudProvider Provider { get; set; }
        public string Region { get; set; }
        public DeploymentConfig Config { get; set; }
        public int Priority { get; set; }
        public double TrafficPercentage { get; set; }
    }

    public class DeploymentResult
    {
        public string DeploymentId { get; set; }
        public DeploymentStatus Status { get; set; }
        public CloudProvider Provider { get; set; }
        public string Region { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public List<DeployedResource> Resources { get; set; }
        public List<string> Errors { get; set; }
        public Dictionary<string, string> Outputs { get; set; }
    }

    public class DeployedResource
    {
        public string ResourceId { get; set; }
        public ResourceType Type { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
        public string Endpoint { get; set; }
        public Dictionary<string, string> Properties { get; set; }
    }

    // Scaling Models
    public class ScalingPolicy
    {
        public string ResourceId { get; set; }
        public string MetricName { get; set; }
        public double ScaleUpThreshold { get; set; }
        public double ScaleDownThreshold { get; set; }
        public int MinInstances { get; set; }
        public int MaxInstances { get; set; }
        public TimeSpan CooldownPeriod { get; set; }
        public ScalingAction ScaleUpAction { get; set; }
        public ScalingAction ScaleDownAction { get; set; }
    }

    public class ScalingAction
    {
        public ScalingDirection Direction { get; set; }
        public int Amount { get; set; }
        public string Unit { get; set; } // instances, cpu, memory
    }

    public class ScalingResult
    {
        public string ScalingEventId { get; set; }
        public bool Success { get; set; }
        public ScalingDirection Direction { get; set; }
        public int PreviousCapacity { get; set; }
        public int NewCapacity { get; set; }
        public string Reason { get; set; }
        public DateTime ExecutedAt { get; set; }
    }

    public class ScalingMetrics
    {
        public string ResourceId { get; set; }
        public int CurrentInstances { get; set; }
        public double CpuUtilization { get; set; }
        public double MemoryUtilization { get; set; }
        public double NetworkUtilization { get; set; }
        public int RequestsPerSecond { get; set; }
        public DateTime LastScalingEvent { get; set; }
    }

    public class ScalingEvent
    {
        public DateTime Timestamp { get; set; }
        public ScalingDirection Direction { get; set; }
        public string Trigger { get; set; }
        public int InstanceChange { get; set; }
        public string Reason { get; set; }
    }

    // Failover Models
    public class FailoverStrategy
    {
        public string PrimaryResourceId { get; set; }
        public List<string> BackupResourceIds { get; set; }
        public FailoverTrigger Trigger { get; set; }
        public TimeSpan MaxFailoverTime { get; set; }
        public bool AutoFailback { get; set; }
        public Dictionary<string, object> CustomParameters { get; set; }
    }

    public class FailoverTrigger
    {
        public string TriggerType { get; set; } // health, performance, manual
        public Dictionary<string, double> Thresholds { get; set; }
        public TimeSpan GracePeriod { get; set; }
    }

    public class DisasterRecoveryPlan
    {
        public string PlanId { get; set; }
        public string Name { get; set; }
        public List<RecoveryStep> Steps { get; set; }
        public TimeSpan RecoveryTimeObjective { get; set; }
        public TimeSpan RecoveryPointObjective { get; set; }
        public Dictionary<string, string> ContactInformation { get; set; }
    }

    public class RecoveryStep
    {
        public int Order { get; set; }
        public string Description { get; set; }
        public string Action { get; set; }
        public Dictionary<string, object> Parameters { get; set; }
        public TimeSpan EstimatedDuration { get; set; }
    }

    public class FailoverResult
    {
        public string FailoverId { get; set; }
        public bool Success { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string PrimaryResource { get; set; }
        public string ActiveResource { get; set; }
        public List<string> ExecutedSteps { get; set; }
        public List<string> Errors { get; set; }
    }

    public class FailoverTarget
    {
        public string ResourceId { get; set; }
        public CloudProvider Provider { get; set; }
        public string Region { get; set; }
        public double HealthScore { get; set; }
        public TimeSpan EstimatedFailoverTime { get; set; }
    }

    // Cost Models
    public class CostOptimization
    {
        public string OptimizationId { get; set; }
        public double CurrentMonthlyCost { get; set; }
        public double ProjectedMonthlyCost { get; set; }
        public double PotentialSavings { get; set; }
        public List<CostRecommendation> Recommendations { get; set; }
        public DateTime AnalysisDate { get; set; }
    }

    public class CostAnalysisRequest
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<CloudProvider> Providers { get; set; }
        public List<string> ResourceTypes { get; set; }
        public string GroupBy { get; set; } // provider, resource, region, tag
    }

    public class CostAnalysis
    {
        public string AnalysisId { get; set; }
        public Dictionary<string, double> CostByProvider { get; set; }
        public Dictionary<string, double> CostByResource { get; set; }
        public Dictionary<string, double> CostByRegion { get; set; }
        public List<CostTrend> Trends { get; set; }
        public double TotalCost { get; set; }
    }

    public class CostTrend
    {
        public DateTime Date { get; set; }
        public double Cost { get; set; }
        public string Category { get; set; }
    }

    public class CostRecommendation
    {
        public string RecommendationId { get; set; }
        public string Type { get; set; } // rightsize, terminate, schedule, reserved
        public string ResourceId { get; set; }
        public string Description { get; set; }
        public double PotentialSavings { get; set; }
        public string Impact { get; set; } // low, medium, high
        public Dictionary<string, object> Parameters { get; set; }
    }

    // Resource Models
    public class CloudResource
    {
        public string ResourceId { get; set; }
        public string Name { get; set; }
        public ResourceType Type { get; set; }
        public CloudProvider Provider { get; set; }
        public string Region { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public Dictionary<string, string> Tags { get; set; }
        public ResourceCost Cost { get; set; }
    }

    public class ResourceCost
    {
        public double HourlyCost { get; set; }
        public double MonthlyCost { get; set; }
        public string Currency { get; set; }
    }

    public class ResourceUtilization
    {
        public string ResourceId { get; set; }
        public double CpuUtilization { get; set; }
        public double MemoryUtilization { get; set; }
        public double StorageUtilization { get; set; }
        public double NetworkUtilization { get; set; }
        public DateTime LastUpdated { get; set; }
        public List<UtilizationDataPoint> History { get; set; }
    }

    public class UtilizationDataPoint
    {
        public DateTime Timestamp { get; set; }
        public Dictionary<string, double> Metrics { get; set; }
    }

    // Load Balancer Models
    public class LoadBalancerRequest
    {
        public string Name { get; set; }
        public LoadBalancerType Type { get; set; }
        public List<string> TargetResourceIds { get; set; }
        public HealthCheckConfig HealthCheck { get; set; }
        public LoadBalancingAlgorithm Algorithm { get; set; }
        public Dictionary<string, string> Tags { get; set; }
    }

    public enum LoadBalancerType
    {
        Application,
        Network,
        Classic
    }

    public enum LoadBalancingAlgorithm
    {
        RoundRobin,
        LeastConnections,
        WeightedRoundRobin,
        IPHash,
        LeastResponseTime
    }

    public class LoadBalancerConfig
    {
        public string LoadBalancerId { get; set; }
        public List<LoadBalancerTarget> Targets { get; set; }
        public HealthCheckConfig HealthCheck { get; set; }
        public LoadBalancingAlgorithm Algorithm { get; set; }
        public bool StickySession { get; set; }
        public int SessionTimeout { get; set; }
    }

    public class LoadBalancerTarget
    {
        public string ResourceId { get; set; }
        public string IpAddress { get; set; }
        public int Port { get; set; }
        public int Weight { get; set; }
        public bool Enabled { get; set; }
    }

    public class HealthCheckConfig
    {
        public string Protocol { get; set; }
        public string Path { get; set; }
        public int Port { get; set; }
        public TimeSpan Interval { get; set; }
        public TimeSpan Timeout { get; set; }
        public int HealthyThreshold { get; set; }
        public int UnhealthyThreshold { get; set; }
    }

    public class LoadBalancerHealth
    {
        public string LoadBalancerId { get; set; }
        public string Status { get; set; }
        public List<TargetHealth> TargetHealth { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    public class TargetHealth
    {
        public string TargetId { get; set; }
        public string Status { get; set; } // healthy, unhealthy, draining
        public string Description { get; set; }
    }

    public class LoadBalancerMetrics
    {
        public string LoadBalancerId { get; set; }
        public int ActiveConnections { get; set; }
        public int RequestsPerSecond { get; set; }
        public double ResponseTime { get; set; }
        public double ErrorRate { get; set; }
        public Dictionary<string, double> TargetMetrics { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    // Supporting Models
    public class ResourceRequirements
    {
        public string InstanceType { get; set; }
        public int CpuCores { get; set; }
        public int MemoryGB { get; set; }
        public int StorageGB { get; set; }
        public string StorageType { get; set; }
    }

    public class NetworkConfiguration
    {
        public string VpcId { get; set; }
        public string SubnetId { get; set; }
        public List<string> SecurityGroupIds { get; set; }
        public bool PublicIpEnabled { get; set; }
    }

    public class SecurityConfiguration
    {
        public List<SecurityRule> InboundRules { get; set; }
        public List<SecurityRule> OutboundRules { get; set; }
        public string KeyPairName { get; set; }
        public bool IamRoleEnabled { get; set; }
    }

    public class SecurityRule
    {
        public string Protocol { get; set; }
        public int FromPort { get; set; }
        public int ToPort { get; set; }
        public string Source { get; set; }
        public string Description { get; set; }
    }

    public class LoadDistributionStrategy
    {
        public string Algorithm { get; set; }
        public Dictionary<CloudProvider, double> ProviderWeights { get; set; }
        public bool EnableFailover { get; set; }
    }

    public class FailoverConfiguration
    {
        public bool Enabled { get; set; }
        public TimeSpan HealthCheckInterval { get; set; }
        public int MaxRetries { get; set; }
        public TimeSpan RetryInterval { get; set; }
    }

    public class DataReplicationStrategy
    {
        public string Strategy { get; set; } // sync, async, lazy
        public List<string> ReplicationTargets { get; set; }
        public TimeSpan ReplicationInterval { get; set; }
    }

    public class HealthCheckResult
    {
        public string ResourceId { get; set; }
        public bool IsHealthy { get; set; }
        public double ResponseTime { get; set; }
        public List<HealthCheck> Checks { get; set; }
        public DateTime CheckedAt { get; set; }
    }

    public class HealthCheck
    {
        public string CheckName { get; set; }
        public bool Passed { get; set; }
        public string Message { get; set; }
        public Dictionary<string, object> Details { get; set; }
    }
}