using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
        
        // Container Registry
        Task<ImagePushResult> PushImageToRegistryAsync(ContainerImage image);
        Task<ContainerImage> PullImageFromRegistryAsync(string imageName, string tag);
        Task<List<ContainerImage>> ListImagesInRegistryAsync();
        Task<bool> DeleteImageFromRegistryAsync(string imageName, string tag);
        
        // Orchestration Management
        Task<List<ContainerService>> GetContainerServicesAsync();
        Task<ContainerServiceHealth> GetServiceHealthAsync(string serviceId);
        Task<ContainerLogs> GetContainerLogsAsync(string containerId, LogQueryOptions options);
        Task<bool> RestartContainerServiceAsync(string serviceId);
        
        // Network Management
        Task<ContainerNetwork> CreateContainerNetworkAsync(NetworkConfiguration config);
        Task<bool> AttachContainerToNetworkAsync(string containerId, string networkId);
        Task<List<ContainerNetwork>> GetContainerNetworksAsync();
        Task<bool> DeleteContainerNetworkAsync(string networkId);
    }

    // Container Deployment Models
    public class ContainerDeploymentConfig
    {
        public string ServiceName { get; set; }
        public string ImageName { get; set; }
        public string ImageTag { get; set; }
        public int Replicas { get; set; }
        public Dictionary<string, string> EnvironmentVariables { get; set; }
        public List<PortMapping> Ports { get; set; }
        public ResourceRequirements Resources { get; set; }
        public Dictionary<string, string> Labels { get; set; }
        public HealthCheckConfiguration HealthCheck { get; set; }
    }

    public class KubernetesDeploymentConfig : ContainerDeploymentConfig
    {
        public string Namespace { get; set; }
        public string DeploymentStrategy { get; set; }
        public Dictionary<string, string> Annotations { get; set; }
        public List<VolumeMount> VolumeMounts { get; set; }
        public List<ConfigMapReference> ConfigMaps { get; set; }
        public List<SecretReference> Secrets { get; set; }
        public ServiceConfiguration Service { get; set; }
        public IngressConfiguration Ingress { get; set; }
    }

    public class ContainerDeploymentResult
    {
        public string DeploymentId { get; set; }
        public string ServiceId { get; set; }
        public string Status { get; set; }
        public DateTime DeployedAt { get; set; }
        public List<ContainerInstance> Containers { get; set; }
        public string ServiceEndpoint { get; set; }
        public Dictionary<string, string> Outputs { get; set; }
        public List<string> Errors { get; set; }
    }

    public class ContainerInstance
    {
        public string ContainerId { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
        public string NodeId { get; set; }
        public string IpAddress { get; set; }
        public DateTime StartedAt { get; set; }
        public Dictionary<string, object> RuntimeInfo { get; set; }
    }

    public class ContainerUpdateConfig
    {
        public string NewImageTag { get; set; }
        public int? NewReplicas { get; set; }
        public Dictionary<string, string> NewEnvironmentVariables { get; set; }
        public ResourceRequirements NewResources { get; set; }
        public string UpdateStrategy { get; set; }
    }

    // Service Mesh Models
    public class ServiceMeshRequest
    {
        public string MeshType { get; set; } // istio, linkerd, consul
        public List<string> Services { get; set; }
        public ServiceMeshConfiguration Configuration { get; set; }
    }

    public class ServiceMeshConfiguration
    {
        public bool EnableTracing { get; set; }
        public bool EnableMetrics { get; set; }
        public bool EnableMutualTLS { get; set; }
        public TrafficManagementConfig TrafficManagement { get; set; }
        public SecurityConfig Security { get; set; }
    }

    public class ServiceMeshConfig
    {
        public string MeshId { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public List<string> ManagedServices { get; set; }
        public DateTime ConfiguredAt { get; set; }
        public Dictionary<string, object> Settings { get; set; }
    }

    public class ServiceMeshMetrics
    {
        public Dictionary<string, double> RequestMetrics { get; set; }
        public Dictionary<string, double> ErrorRates { get; set; }
        public Dictionary<string, double> Latencies { get; set; }
        public Dictionary<string, int> ServiceConnections { get; set; }
        public DateTime CollectedAt { get; set; }
    }

    public class ServiceMeshPolicy
    {
        public string PolicyId { get; set; }
        public string Type { get; set; }
        public List<string> TargetServices { get; set; }
        public Dictionary<string, object> Rules { get; set; }
    }

    public class TrafficManagementConfig
    {
        public bool EnableLoadBalancing { get; set; }
        public string LoadBalancingAlgorithm { get; set; }
        public bool EnableRetries { get; set; }
        public int MaxRetries { get; set; }
        public TimeSpan Timeout { get; set; }
    }

    public class SecurityConfig
    {
        public bool EnforceAuthz { get; set; }
        public bool EnableRBAC { get; set; }
        public List<string> AllowedOrigins { get; set; }
        public Dictionary<string, string> SecurityPolicies { get; set; }
    }

    // Container Registry Models
    public class ContainerImage
    {
        public string ImageId { get; set; }
        public string Repository { get; set; }
        public string Tag { get; set; }
        public string Digest { get; set; }
        public long Size { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime PushedAt { get; set; }
        public Dictionary<string, string> Labels { get; set; }
        public List<string> Layers { get; set; }
        public ImageManifest Manifest { get; set; }
    }

    public class ImageManifest
    {
        public string Architecture { get; set; }
        public string OS { get; set; }
        public List<string> Layers { get; set; }
        public Dictionary<string, object> Config { get; set; }
    }

    public class ImagePushResult
    {
        public bool Success { get; set; }
        public string ImageId { get; set; }
        public string Repository { get; set; }
        public string Tag { get; set; }
        public string Digest { get; set; }
        public DateTime PushedAt { get; set; }
        public List<string> Errors { get; set; }
    }

    // Service Management Models
    public class ContainerService
    {
        public string ServiceId { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
        public int DesiredReplicas { get; set; }
        public int RunningReplicas { get; set; }
        public string ImageName { get; set; }
        public string ImageTag { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastUpdated { get; set; }
        public List<ServicePort> Ports { get; set; }
        public Dictionary<string, string> Labels { get; set; }
    }

    public class ServicePort
    {
        public int Port { get; set; }
        public int TargetPort { get; set; }
        public string Protocol { get; set; }
        public string Name { get; set; }
    }

    public class ContainerServiceHealth
    {
        public string ServiceId { get; set; }
        public string OverallStatus { get; set; }
        public List<ContainerHealth> ContainerHealth { get; set; }
        public ServiceMetrics Metrics { get; set; }
        public DateTime LastChecked { get; set; }
    }

    public class ContainerHealth
    {
        public string ContainerId { get; set; }
        public string Status { get; set; }
        public bool IsReady { get; set; }
        public int RestartCount { get; set; }
        public DateTime LastRestart { get; set; }
        public List<HealthCheckResult> HealthChecks { get; set; }
    }

    public class ServiceMetrics
    {
        public double CpuUsage { get; set; }
        public double MemoryUsage { get; set; }
        public double NetworkInput { get; set; }
        public double NetworkOutput { get; set; }
        public int RequestCount { get; set; }
        public double ResponseTime { get; set; }
    }

    public class ContainerLogs
    {
        public string ContainerId { get; set; }
        public List<LogEntry> Entries { get; set; }
        public DateTime FromTime { get; set; }
        public DateTime ToTime { get; set; }
        public int TotalLines { get; set; }
    }

    public class LogEntry
    {
        public DateTime Timestamp { get; set; }
        public string Level { get; set; }
        public string Message { get; set; }
        public Dictionary<string, object> Context { get; set; }
    }

    public class LogQueryOptions
    {
        public DateTime? FromTime { get; set; }
        public DateTime? ToTime { get; set; }
        public int? TailLines { get; set; }
        public bool Follow { get; set; }
        public string LogLevel { get; set; }
        public string Filter { get; set; }
    }

    // Network Management Models
    public class ContainerNetwork
    {
        public string NetworkId { get; set; }
        public string Name { get; set; }
        public string Driver { get; set; }
        public string Scope { get; set; }
        public NetworkConfiguration Configuration { get; set; }
        public List<string> AttachedContainers { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class NetworkConfiguration
    {
        public string Subnet { get; set; }
        public string Gateway { get; set; }
        public Dictionary<string, string> Options { get; set; }
        public List<string> DNSServers { get; set; }
        public bool EnableIPv6 { get; set; }
    }

    // Supporting Models
    public class PortMapping
    {
        public int HostPort { get; set; }
        public int ContainerPort { get; set; }
        public string Protocol { get; set; }
    }

    public class HealthCheckConfiguration
    {
        public string Type { get; set; } // http, tcp, exec
        public string Endpoint { get; set; }
        public TimeSpan Interval { get; set; }
        public TimeSpan Timeout { get; set; }
        public int Retries { get; set; }
        public TimeSpan StartPeriod { get; set; }
    }

    public class VolumeMount
    {
        public string Name { get; set; }
        public string MountPath { get; set; }
        public string SubPath { get; set; }
        public bool ReadOnly { get; set; }
    }

    public class ConfigMapReference
    {
        public string Name { get; set; }
        public Dictionary<string, string> Data { get; set; }
    }

    public class SecretReference
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public Dictionary<string, byte[]> Data { get; set; }
    }

    public class ServiceConfiguration
    {
        public string Type { get; set; } // ClusterIP, NodePort, LoadBalancer
        public List<ServicePort> Ports { get; set; }
        public Dictionary<string, string> Selector { get; set; }
    }

    public class IngressConfiguration
    {
        public string ClassName { get; set; }
        public List<IngressRule> Rules { get; set; }
        public TLSConfiguration TLS { get; set; }
    }

    public class IngressRule
    {
        public string Host { get; set; }
        public List<IngressPath> Paths { get; set; }
    }

    public class IngressPath
    {
        public string Path { get; set; }
        public string PathType { get; set; }
        public string ServiceName { get; set; }
        public int ServicePort { get; set; }
    }

    public class TLSConfiguration
    {
        public List<string> Hosts { get; set; }
        public string SecretName { get; set; }
    }
}