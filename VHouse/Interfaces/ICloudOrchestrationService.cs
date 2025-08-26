using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VHouse.Classes;

namespace VHouse.Interfaces
{
    public enum CloudProvider
    {
        AWS,
        Azure,
        GCP,
        DigitalOcean,
        Hybrid
    }
    
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
}