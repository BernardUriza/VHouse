using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VHouse.Classes;

namespace VHouse.Interfaces
{
    public interface IContainerOrchestrationService
    {
        // Container Deployment
        Task<ContainerDeploymentResult> DeployContainerAsync(ContainerDeploymentConfig config);
        Task<ContainerDeploymentResult> DeployToKubernetesAsync(KubernetesDeploymentConfig config);
        Task<bool> ScaleContainerServiceAsync(string serviceId, int replicas);
        Task<bool> UpdateContainerServiceAsync(string serviceId, ContainerUpdateConfig updateConfig);
        
        // Service Mesh Management
        Task<ServiceMeshConfig> ConfigureServiceMeshAsync(ServiceMeshRequest request);
        Task<ServiceMeshMetrics> GetServiceMeshMetricsAsync();
        Task<bool> UpdateServiceMeshPolicyAsync(ServiceMeshPolicy policy);
        
        // Container Registry Management
        Task<ImagePushResult> PushImageToRegistryAsync(ContainerImage image);
        Task<ContainerImage> PullImageFromRegistryAsync(string imageName, string tag);
        Task<List<ContainerImage>> ListImagesInRegistryAsync();
        Task<bool> DeleteImageFromRegistryAsync(string imageName, string tag);
        
        // Health Monitoring
        Task<ContainerServiceHealth> GetServiceHealthAsync(string serviceId);
        Task<ContainerLogs> GetContainerLogsAsync(string containerId, LogQueryOptions options);
        
        // Network Management
        Task<ContainerNetwork> CreateContainerNetworkAsync(ContainerNetworkConfiguration config);
        Task<bool> AttachContainerToNetworkAsync(string containerId, string networkId);
        Task<List<ContainerNetwork>> GetContainerNetworksAsync();
        Task<bool> DeleteContainerNetworkAsync(string networkId);
    }
}