using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace VHouse.Interfaces
{
    public interface IInfrastructureService
    {
        // Infrastructure as Code (IaC)
        Task<ProvisioningResult> ProvisionInfrastructureAsync(InfrastructureTemplate template);
        Task<ProvisioningResult> UpdateInfrastructureAsync(string infrastructureId, InfrastructureTemplate template);
        Task<bool> DestroyInfrastructureAsync(string infrastructureId);
        Task<List<InfrastructureStack>> GetInfrastructureStacksAsync();
        
        // Health Monitoring
        Task<HealthStatus> MonitorInfrastructureHealthAsync();
        Task<List<HealthAlert>> GetHealthAlertsAsync();
        Task<InfrastructureMetrics> GetInfrastructureMetricsAsync();
        Task<bool> SetHealthCheckPolicyAsync(HealthCheckPolicy policy);
        
        // Configuration Management
        Task<ConfigurationDrift> DetectConfigurationDriftAsync();
        Task<bool> ApplyConfigurationUpdatesAsync(string infrastructureId, Dictionary<string, object> updates);
        Task<ConfigurationBackup> BackupConfigurationAsync(string infrastructureId);
        Task<bool> RestoreConfigurationAsync(string infrastructureId, string backupId);
        
        // Compliance & Security
        Task<ComplianceReport> RunComplianceChecksAsync();
        Task<SecurityAssessment> PerformSecurityAssessmentAsync();
        Task<bool> ApplySecurityPolicyAsync(SecurityPolicy policy);
        Task<List<ComplianceViolation>> GetComplianceViolationsAsync();
        
        // Resource Lifecycle
        Task<List<ResourceLifecycleEvent>> GetResourceLifecycleEventsAsync(string resourceId);
        Task<bool> ScheduleMaintenanceAsync(MaintenanceSchedule schedule);
        Task<List<MaintenanceWindow>> GetMaintenanceWindowsAsync();
        Task<bool> ExecuteMaintenanceTaskAsync(string taskId);
    }

    // Core Models
    public class InfrastructureTemplate
    {
        public string TemplateId { get; set; }
        public string Name { get; set; }
        public string Version { get; set; }
        public string Provider { get; set; } // terraform, cloudformation, arm, etc.
        public Dictionary<string, object> Parameters { get; set; }
        public Dictionary<string, ResourceDefinition> Resources { get; set; }
        public Dictionary<string, string> Outputs { get; set; }
        public Dictionary<string, string> Tags { get; set; }
    }

    public class ResourceDefinition
    {
        public string Type { get; set; }
        public Dictionary<string, object> Properties { get; set; }
        public List<string> DependsOn { get; set; }
    }

    public class ProvisioningResult
    {
        public string InfrastructureId { get; set; }
        public string Status { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public List<ProvisionedResource> Resources { get; set; }
        public Dictionary<string, string> Outputs { get; set; }
        public List<string> Errors { get; set; }
        public string ProviderExecutionId { get; set; }
    }

    public class ProvisionedResource
    {
        public string ResourceId { get; set; }
        public string LogicalId { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public Dictionary<string, object> Properties { get; set; }
    }

    public class InfrastructureStack
    {
        public string StackId { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastUpdated { get; set; }
        public string TemplateVersion { get; set; }
        public List<string> ResourceIds { get; set; }
        public Dictionary<string, string> Outputs { get; set; }
    }

    // Health Monitoring Models
    public class HealthStatus
    {
        public string OverallStatus { get; set; }
        public DateTime CheckedAt { get; set; }
        public List<ComponentHealth> Components { get; set; }
        public Dictionary<string, double> HealthScores { get; set; }
        public int TotalComponents { get; set; }
        public int HealthyComponents { get; set; }
        public int UnhealthyComponents { get; set; }
    }

    public class ComponentHealth
    {
        public string ComponentId { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public double HealthScore { get; set; }
        public List<HealthMetric> Metrics { get; set; }
        public DateTime LastChecked { get; set; }
    }

    public class HealthMetric
    {
        public string Name { get; set; }
        public double Value { get; set; }
        public string Unit { get; set; }
        public double Threshold { get; set; }
        public bool IsHealthy { get; set; }
    }

    public class HealthAlert
    {
        public string AlertId { get; set; }
        public string Severity { get; set; }
        public string ComponentId { get; set; }
        public string Message { get; set; }
        public DateTime TriggeredAt { get; set; }
        public bool IsResolved { get; set; }
        public Dictionary<string, object> Context { get; set; }
    }

    public class InfrastructureMetrics
    {
        public Dictionary<string, double> SystemMetrics { get; set; }
        public Dictionary<string, double> ResourceUtilization { get; set; }
        public Dictionary<string, double> PerformanceMetrics { get; set; }
        public Dictionary<string, int> ResourceCounts { get; set; }
        public DateTime CollectedAt { get; set; }
    }

    public class HealthCheckPolicy
    {
        public string PolicyId { get; set; }
        public string Name { get; set; }
        public List<HealthCheckRule> Rules { get; set; }
        public TimeSpan CheckInterval { get; set; }
        public int RetryCount { get; set; }
        public TimeSpan RetryInterval { get; set; }
    }

    public class HealthCheckRule
    {
        public string MetricName { get; set; }
        public string Operator { get; set; }
        public double Threshold { get; set; }
        public string Action { get; set; }
    }

    // Configuration Management Models
    public class ConfigurationDrift
    {
        public string DriftId { get; set; }
        public DateTime DetectedAt { get; set; }
        public List<DriftItem> DriftItems { get; set; }
        public string Severity { get; set; }
        public bool RequiresAction { get; set; }
    }

    public class DriftItem
    {
        public string ResourceId { get; set; }
        public string Property { get; set; }
        public object ExpectedValue { get; set; }
        public object ActualValue { get; set; }
        public string DriftType { get; set; }
    }

    public class ConfigurationBackup
    {
        public string BackupId { get; set; }
        public string InfrastructureId { get; set; }
        public DateTime CreatedAt { get; set; }
        public Dictionary<string, object> Configuration { get; set; }
        public string BackupSize { get; set; }
        public string StorageLocation { get; set; }
    }

    // Compliance Models
    public class ComplianceReport
    {
        public string ReportId { get; set; }
        public DateTime GeneratedAt { get; set; }
        public string OverallScore { get; set; }
        public List<ComplianceCheck> Checks { get; set; }
        public Dictionary<string, int> ViolationsByCategory { get; set; }
        public List<ComplianceRecommendation> Recommendations { get; set; }
    }

    public class ComplianceCheck
    {
        public string CheckId { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public string Status { get; set; }
        public string Severity { get; set; }
        public string Description { get; set; }
        public List<string> AffectedResources { get; set; }
    }

    public class ComplianceViolation
    {
        public string ViolationId { get; set; }
        public string ResourceId { get; set; }
        public string RuleId { get; set; }
        public string Severity { get; set; }
        public string Description { get; set; }
        public DateTime DetectedAt { get; set; }
        public string Status { get; set; }
    }

    public class ComplianceRecommendation
    {
        public string RecommendationId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Priority { get; set; }
        public List<string> ActionItems { get; set; }
    }

    public class SecurityAssessment
    {
        public string AssessmentId { get; set; }
        public DateTime AssessedAt { get; set; }
        public string SecurityScore { get; set; }
        public List<SecurityFinding> Findings { get; set; }
        public Dictionary<string, int> VulnerabilitiesByLevel { get; set; }
        public List<SecurityRecommendation> Recommendations { get; set; }
    }

    public class SecurityFinding
    {
        public string FindingId { get; set; }
        public string Title { get; set; }
        public string Severity { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public List<string> AffectedResources { get; set; }
        public Dictionary<string, object> Evidence { get; set; }
    }

    public class SecurityRecommendation
    {
        public string RecommendationId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Priority { get; set; }
        public List<string> RemediationSteps { get; set; }
    }

    public class SecurityPolicy
    {
        public string PolicyId { get; set; }
        public string Name { get; set; }
        public Dictionary<string, object> Rules { get; set; }
        public List<string> ApplicableResources { get; set; }
        public string Enforcement { get; set; }
    }

    // Lifecycle Management Models
    public class ResourceLifecycleEvent
    {
        public string EventId { get; set; }
        public string ResourceId { get; set; }
        public string EventType { get; set; }
        public DateTime Timestamp { get; set; }
        public Dictionary<string, object> Details { get; set; }
        public string InitiatedBy { get; set; }
    }

    public class MaintenanceSchedule
    {
        public string ScheduleId { get; set; }
        public string Name { get; set; }
        public DateTime ScheduledAt { get; set; }
        public TimeSpan Duration { get; set; }
        public List<string> AffectedResources { get; set; }
        public List<MaintenanceTask> Tasks { get; set; }
        public string NotificationList { get; set; }
    }

    public class MaintenanceTask
    {
        public string TaskId { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public Dictionary<string, object> Parameters { get; set; }
        public TimeSpan EstimatedDuration { get; set; }
        public int Order { get; set; }
    }

    public class MaintenanceWindow
    {
        public string WindowId { get; set; }
        public string Name { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Status { get; set; }
        public List<string> ScheduledTasks { get; set; }
        public string RecurrencePattern { get; set; }
    }
}