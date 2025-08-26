using VHouse.Classes;

namespace VHouse.Interfaces;

public interface IAPIGatewayService
{
    Task<APIResponse> RouteRequestAsync(APIRequest request, RoutingConfig config);
    Task<RateLimitResult> ApplyRateLimitingAsync(string clientId, RateLimitPolicy policy);
    Task<AuthenticationResult> AuthenticateAPIRequestAsync(APICredentials credentials);
    Task<APIAnalytics> GetAPIUsageAnalyticsAsync(AnalyticsQuery query);
    Task<VersioningResult> ManageAPIVersioningAsync(APIVersionRequest request);
    Task<ThrottlingResult> ApplyThrottlingAsync(string clientId, ThrottlingPolicy policy);
    Task<CachingResult> ManageAPICachingAsync(CacheRequest request);
    Task<LoadBalancingResult> BalanceLoadAsync(LoadBalancingRequest request);
    Task<APIKey> GenerateAPIKeyAsync(APIKeyRequest request);
    Task<ValidationResult> ValidateAPIRequestAsync(APIValidationRequest request);
    Task<TransformationResult> TransformAPIRequestAsync(TransformationRequest request);
    Task<MonitoringResult> MonitorAPIHealthAsync(MonitoringRequest request);
    Task<QuotaResult> ManageAPIQuotaAsync(QuotaRequest request);
    Task<SecurityResult> ApplyAPISecurityAsync(SecurityRequest request);
    Task<DocumentationResult> GenerateAPIDocumentationAsync(DocumentationRequest request);
}

public interface IAPIManagementService
{
    Task<APIDefinition> CreateAPIDefinitionAsync(APIDefinitionRequest request);
    Task<APIDefinition> UpdateAPIDefinitionAsync(string apiId, APIDefinitionUpdateRequest request);
    Task<APIDefinition> GetAPIDefinitionAsync(string apiId);
    Task<List<APIDefinition>> ListAPIDefinitionsAsync(APIListRequest request);
    Task<bool> DeleteAPIDefinitionAsync(string apiId);
    Task<APIDeployment> DeployAPIAsync(APIDeploymentRequest request);
    Task<APIStatus> GetAPIStatusAsync(string apiId);
    Task<APIMetrics> GetAPIMetricsAsync(string apiId, MetricsRequest request);
    Task<APILog> GetAPILogsAsync(string apiId, LogRequest request);
    Task<TestResult> TestAPIEndpointAsync(EndpointTestRequest request);
}

public interface IPartnerEcosystemService
{
    Task<Developer> RegisterDeveloperAsync(DeveloperRegistration registration);
    Task<Application> CreateApplicationAsync(string developerId, ApplicationRequest request);
    Task<SDK> GenerateSDKAsync(SDKRequest request);
    Task<PartnerPortal> GetPartnerPortalAsync(string partnerId);
    Task<PartnershipRequest> SubmitPartnershipRequestAsync(PartnershipApplication application);
    Task<IntegrationGuide> GetIntegrationGuideAsync(string apiId);
    Task<DeveloperMetrics> GetDeveloperMetricsAsync(string developerId);
    Task<Webhook> RegisterWebhookAsync(WebhookRegistration registration);
    Task<SandboxEnvironment> CreateSandboxAsync(SandboxRequest request);
    Task<Certification> GetAPICertificationAsync(CertificationRequest request);
}