using System.ComponentModel.DataAnnotations;

namespace VHouse.Classes;

// Blockchain Core Models
public class BlockchainWallet
{
    public string WalletId { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string PublicKey { get; set; } = string.Empty;
    public string PrivateKey { get; set; } = string.Empty;
    public string OwnerName { get; set; } = string.Empty;
    public string WalletType { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
    public decimal Balance { get; set; }
    public string Network { get; set; } = string.Empty;
    public string SecurityLevel { get; set; } = string.Empty;
}

public class WalletCreationRequest
{
    public string OwnerName { get; set; } = string.Empty;
    public string WalletType { get; set; } = "Standard";
    public bool IncludePrivateKey { get; set; } = false;
    public string? Network { get; set; }
    public string SecurityLevel { get; set; } = "High";
    public Dictionary<string, object> Metadata { get; set; } = new();
}

// Smart Contract Models
public class ContractCreationRequest
{
    public string ContractName { get; set; } = string.Empty;
    public string SourceCode { get; set; } = string.Empty;
    public string CompilerVersion { get; set; } = string.Empty;
    public Dictionary<string, object> ConstructorParameters { get; set; } = new();
    public decimal GasLimit { get; set; }
    public string CreatorAddress { get; set; } = string.Empty;
    public Dictionary<string, string> Metadata { get; set; } = new();
}

public class ContractDeployment
{
    public string DeploymentId { get; set; } = string.Empty;
    public string ContractAddress { get; set; } = string.Empty;
    public string TransactionHash { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public long BlockNumber { get; set; }
    public decimal GasUsed { get; set; }
    public DateTime DeployedAt { get; set; }
    public string? ErrorMessage { get; set; }
}

public class ExecutionResult
{
    public string ExecutionId { get; set; } = string.Empty;
    public bool Success { get; set; }
    public Dictionary<string, object> ReturnValues { get; set; } = new();
    public decimal GasUsed { get; set; }
    public string TransactionHash { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public DateTime ExecutedAt { get; set; }
}

public class ContractState
{
    public string ContractAddress { get; set; } = string.Empty;
    public Dictionary<string, object> StateVariables { get; set; } = new();
    public DateTime LastUpdated { get; set; }
    public long BlockNumber { get; set; }
    public string StateHash { get; set; } = string.Empty;
}

// Digital Identity Models
public class DigitalIdentity
{
    public string IdentityId { get; set; } = string.Empty;
    public string DIDDocument { get; set; } = string.Empty;
    public string PublicKey { get; set; } = string.Empty;
    public string OwnerAddress { get; set; } = string.Empty;
    public Dictionary<string, object> Attributes { get; set; } = new();
    public List<string> Verifications { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
}

public class CredentialIssuance
{
    public string CredentialId { get; set; } = string.Empty;
    public string IssuerDID { get; set; } = string.Empty;
    public string SubjectDID { get; set; } = string.Empty;
    public Dictionary<string, object> Claims { get; set; } = new();
    public DateTime IssuedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public string Signature { get; set; } = string.Empty;
    public bool IsRevoked { get; set; }
}

// Tokenization Models
public class TokenCreation
{
    public string TokenName { get; set; } = string.Empty;
    public string TokenSymbol { get; set; } = string.Empty;
    public decimal InitialSupply { get; set; }
    public int Decimals { get; set; } = 18;
    public string TokenType { get; set; } = string.Empty; // ERC20, ERC721, ERC1155
    public string CreatorAddress { get; set; } = string.Empty;
    public Dictionary<string, object> Properties { get; set; } = new();
}

public class TokenMinting
{
    public string TokenContract { get; set; } = string.Empty;
    public string RecipientAddress { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? TokenId { get; set; } // For NFTs
    public Dictionary<string, object> Metadata { get; set; } = new();
    public string MinterAddress { get; set; } = string.Empty;
}

// Integration Connection Models
public class ERPConnectionConfig
{
    public string ConnectionId { get; set; } = string.Empty;
    public string ERPSystem { get; set; } = string.Empty; // SAP, Oracle, etc.
    public string ServerUrl { get; set; } = string.Empty;
    public string Database { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public Dictionary<string, string> ConnectionProperties { get; set; } = new();
    public bool UseSSL { get; set; }
    public int ConnectionTimeout { get; set; } = 30;
}

public class CRMConnection
{
    public string ConnectionId { get; set; } = string.Empty;
    public string CRMPlatform { get; set; } = string.Empty; // Salesforce, HubSpot, etc.
    public string ApiEndpoint { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
    public DateTime TokenExpiry { get; set; }
    public Dictionary<string, string> CustomFields { get; set; } = new();
}

public class ECommerceConnection
{
    public string ConnectionId { get; set; } = string.Empty;
    public string Platform { get; set; } = string.Empty; // Shopify, WooCommerce, etc.
    public string StoreUrl { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string ApiSecret { get; set; } = string.Empty;
    public string WebhookSecret { get; set; } = string.Empty;
    public List<string> SyncEntities { get; set; } = new();
    public Dictionary<string, object> PlatformSettings { get; set; } = new();
}

public class APIKeyManagement
{
    public string KeyId { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string KeySecret { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> Scopes { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool IsActive { get; set; }
    public int RateLimitPerHour { get; set; }
    public string Environment { get; set; } = "Production";
}

public class DeveloperPortal
{
    public string PortalId { get; set; } = string.Empty;
    public string DeveloperName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Company { get; set; } = string.Empty;
    public List<string> APIKeys { get; set; } = new();
    public List<string> SubscribedAPIs { get; set; } = new();
    public Dictionary<string, object> Profile { get; set; } = new();
    public DateTime RegistrationDate { get; set; }
    public bool IsVerified { get; set; }
    public string Status { get; set; } = "Active";
}

// Additional Blockchain models
public class VotingRequest
{
    public string VoteId { get; set; } = string.Empty;
    public string ProposalId { get; set; } = string.Empty;
    public string VoterAddress { get; set; } = string.Empty;
    public string Vote { get; set; } = string.Empty; // YES, NO, ABSTAIN
    public Dictionary<string, object> VoteDetails { get; set; } = new();
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
}

public class GovernanceVote
{
    public string VoteId { get; set; } = string.Empty;
    public string ProposalId { get; set; } = string.Empty;
    public string VoterAddress { get; set; } = string.Empty;
    public string Vote { get; set; } = string.Empty;
    public double VotingPower { get; set; }
    public string TransactionHash { get; set; } = string.Empty;
    public DateTime VotedAt { get; set; } = DateTime.UtcNow;
    public bool IsVerified { get; set; }
}

// Additional missing Blockchain models
public class TokenMetadata
{
    public string MetadataId { get; set; } = string.Empty;
    public string TokenContract { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public Dictionary<string, object> Attributes { get; set; } = new();
    public Dictionary<string, string> Properties { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = string.Empty;
}

public class TokenHistory
{
    public string HistoryId { get; set; } = string.Empty;
    public string TokenContract { get; set; } = string.Empty;
    public string TokenId { get; set; } = string.Empty;
    public List<TokenHistoryEntry> Entries { get; set; } = new();
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

public class TokenHistoryEntry
{
    public string TransactionHash { get; set; } = string.Empty;
    public string FromAddress { get; set; } = string.Empty;
    public string ToAddress { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string EventType { get; set; } = string.Empty; // MINT, TRANSFER, BURN
    public DateTime Timestamp { get; set; }
    public long BlockNumber { get; set; }
}

public class StakingRequest
{
    public string RequestId { get; set; } = string.Empty;
    public string StakerAddress { get; set; } = string.Empty;
    public string TokenContract { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int LockPeriodDays { get; set; }
    public string StakingPool { get; set; } = string.Empty;
    public Dictionary<string, object> Parameters { get; set; } = new();
}

public class StakingResult
{
    public string StakingId { get; set; } = string.Empty;
    public string RequestId { get; set; } = string.Empty;
    public string TransactionHash { get; set; } = string.Empty;
    public bool Success { get; set; }
    public decimal AmountStaked { get; set; }
    public decimal ExpectedReward { get; set; }
    public DateTime StartDate { get; set; } = DateTime.UtcNow;
    public DateTime EndDate { get; set; }
    public string Status { get; set; } = "ACTIVE";
}

public class RewardRequest
{
    public string RequestId { get; set; } = string.Empty;
    public string RewardType { get; set; } = string.Empty;
    public List<string> RecipientAddresses { get; set; } = new();
    public decimal TotalRewardAmount { get; set; }
    public string RewardToken { get; set; } = string.Empty;
    public Dictionary<string, object> DistributionCriteria { get; set; } = new();
}

public class RewardDistribution
{
    public string DistributionId { get; set; } = string.Empty;
    public string RequestId { get; set; } = string.Empty;
    public List<RewardAllocation> Allocations { get; set; } = new();
    public decimal TotalDistributed { get; set; }
    public DateTime DistributedAt { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "COMPLETED";
    public List<string> TransactionHashes { get; set; } = new();
}

public class RewardAllocation
{
    public string RecipientAddress { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string RewardType { get; set; } = string.Empty;
    public string TransactionHash { get; set; } = string.Empty;
    public DateTime AllocatedAt { get; set; } = DateTime.UtcNow;
}

public class TokenTransaction
{
    public string TransactionId { get; set; } = string.Empty;
    public string TransactionHash { get; set; } = string.Empty;
    public string FromAddress { get; set; } = string.Empty;
    public string ToAddress { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string TokenContract { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public long BlockNumber { get; set; }
    public decimal GasUsed { get; set; }
    public decimal GasFee { get; set; }
    public DateTime Timestamp { get; set; }
    public string? ErrorMessage { get; set; }
}

public class TokenTransferRequest
{
    public string FromAddress { get; set; } = string.Empty;
    public string ToAddress { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string TokenContract { get; set; } = string.Empty;
    public decimal GasLimit { get; set; } = 100000;
    public decimal GasPrice { get; set; } = 20;
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public class SmartContract
{
    public string ContractId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string DeploymentHash { get; set; } = string.Empty;
    public string ByteCode { get; set; } = string.Empty;
    public string ABI { get; set; } = string.Empty;
    public string Owner { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
    public decimal GasUsedForDeployment { get; set; }
    public string Network { get; set; } = string.Empty;
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public class SmartContractDeployment
{
    public string ContractName { get; set; } = string.Empty;
    public string SourceCode { get; set; } = string.Empty;
    public string DeployerAddress { get; set; } = string.Empty;
    public string Version { get; set; } = "1.0.0";
    public List<object> ContractABI { get; set; } = new();
    public Dictionary<string, object> ConstructorParameters { get; set; } = new();
    public decimal GasLimit { get; set; } = 2000000;
    public decimal GasPrice { get; set; } = 20;
    public string? Network { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public class BlockchainTransaction
{
    public string Hash { get; set; } = string.Empty;
    public string BlockHash { get; set; } = string.Empty;
    public long BlockNumber { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal GasUsed { get; set; }
    public decimal GasPrice { get; set; }
    public DateTime Timestamp { get; set; }
    public bool Success { get; set; }
    public Dictionary<string, object> Data { get; set; } = new();
}

public class BlockchainBlock
{
    public string Hash { get; set; } = string.Empty;
    public long Number { get; set; }
    public string PreviousHash { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public int TransactionCount { get; set; }
    public List<string> Transactions { get; set; } = new();
    public string Miner { get; set; } = string.Empty;
    public long Difficulty { get; set; }
    public long Size { get; set; }
    public decimal GasUsed { get; set; }
    public decimal GasLimit { get; set; }
}

public class ConsensusRequest
{
    public string BlockHash { get; set; } = string.Empty;
    public string ValidatorAddress { get; set; } = string.Empty;
    public string ConsensusType { get; set; } = "Proof_of_Stake";
    public Dictionary<string, object> ValidationData { get; set; } = new();
}

public class ConsensusResult
{
    public string BlockHash { get; set; } = string.Empty;
    public string ParticipantAddress { get; set; } = string.Empty;
    public bool ConsensusReached { get; set; }
    public int VotingPower { get; set; }
    public decimal RewardEarned { get; set; }
    public DateTime ParticipationTime { get; set; }
    public decimal NetworkFees { get; set; }
    public string ConsensusAlgorithm { get; set; } = string.Empty;
}

public class NetworkStatus
{
    public string NetworkName { get; set; } = string.Empty;
    public int ChainId { get; set; }
    public long LatestBlockNumber { get; set; }
    public int PeerCount { get; set; }
    public int TransactionPoolSize { get; set; }
    public TimeSpan AverageBlockTime { get; set; }
    public long NetworkHashRate { get; set; }
    public bool IsHealthy { get; set; }
    public double SyncProgress { get; set; }
    public DateTime LastBlockTimestamp { get; set; }
}

public class GasEstimationRequest
{
    public string FromAddress { get; set; } = string.Empty;
    public string ToAddress { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public string? Data { get; set; }
    public string? ContractAddress { get; set; }
}

public class GasEstimation
{
    public decimal EstimatedGas { get; set; }
    public decimal GasPrice { get; set; }
    public decimal MaxFeePerGas { get; set; }
    public decimal MaxPriorityFeePerGas { get; set; }
    public decimal EstimatedCost { get; set; }
    public double Confidence { get; set; }
}

public class ContractInteractionRequest
{
    public string ContractAddress { get; set; } = string.Empty;
    public string FunctionName { get; set; } = string.Empty;
    public List<object> Parameters { get; set; } = new();
    public string CallerAddress { get; set; } = string.Empty;
    public decimal GasLimit { get; set; } = 100000;
    public decimal GasPrice { get; set; } = 20;
    public bool ReadOnly { get; set; } = false;
}

public class ContractInteraction
{
    public string InteractionId { get; set; } = string.Empty;
    public string ContractAddress { get; set; } = string.Empty;
    public string FunctionName { get; set; } = string.Empty;
    public bool Success { get; set; }
    public object? Result { get; set; }
    public decimal GasUsed { get; set; }
    public string TransactionHash { get; set; } = string.Empty;
    public DateTime ExecutedAt { get; set; }
    public string? ErrorMessage { get; set; }
}

public class AuditRequest
{
    public string Address { get; set; } = string.Empty;
    public TimeSpan AuditPeriod { get; set; }
    public List<string> AuditTypes { get; set; } = new();
    public Dictionary<string, object> Filters { get; set; } = new();
}

public class BlockchainAudit
{
    public string AuditId { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public TimeSpan AuditPeriod { get; set; }
    public int TransactionCount { get; set; }
    public int SuspiciousTransactions { get; set; }
    public double ComplianceScore { get; set; }
    public string RiskLevel { get; set; } = string.Empty;
    public List<string> AuditFindings { get; set; } = new();
    public DateTime AuditedAt { get; set; }
    public string AuditorSignature { get; set; } = string.Empty;
}

public class ProofRequest
{
    public string ProofType { get; set; } = string.Empty;
    public string DataHash { get; set; } = string.Empty;
    public object Data { get; set; } = new();
    public List<object> PublicInputs { get; set; } = new();
    public Dictionary<string, object> ProofParameters { get; set; } = new();
}

public class CryptographicProof
{
    public string ProofId { get; set; } = string.Empty;
    public string ProofType { get; set; } = string.Empty;
    public string DataHash { get; set; } = string.Empty;
    public string Proof { get; set; } = string.Empty;
    public List<object> PublicInputs { get; set; } = new();
    public string VerificationKey { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsValid { get; set; }
    public int ProofSize { get; set; }
}

public class IdentityVerificationRequest
{
    public string IdentityId { get; set; } = string.Empty;
    public string VerificationMethod { get; set; } = string.Empty;
    public Dictionary<string, object> IdentityData { get; set; } = new();
    public List<string> RequiredAttributes { get; set; } = new();
}

public class IdentityVerification
{
    public string VerificationId { get; set; } = string.Empty;
    public string IdentityId { get; set; } = string.Empty;
    public bool IsVerified { get; set; }
    public string VerificationMethod { get; set; } = string.Empty;
    public double TrustScore { get; set; }
    public Dictionary<string, object> VerificationData { get; set; } = new();
    public DateTime VerifiedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public string? VerificationCertificate { get; set; }
}

// Supply Chain Blockchain Models
public class SupplyChainRecord
{
    public string RecordId { get; set; } = string.Empty;
    public string ProductId { get; set; } = string.Empty;
    public string StepName { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string Location { get; set; } = string.Empty;
    public string ParticipantId { get; set; } = string.Empty;
    public Dictionary<string, object> StepData { get; set; } = new();
    public string TransactionHash { get; set; } = string.Empty;
    public string PreviousRecordHash { get; set; } = string.Empty;
}

public class SupplyChainStepRequest
{
    public string ProductId { get; set; } = string.Empty;
    public string StepName { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string ParticipantId { get; set; } = string.Empty;
    public Dictionary<string, object> StepData { get; set; } = new();
    public List<string> Certifications { get; set; } = new();
}

public class ProductJourney
{
    public string ProductId { get; set; } = string.Empty;
    public List<SupplyChainRecord> Steps { get; set; } = new();
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int TotalSteps { get; set; }
    public List<string> Participants { get; set; } = new();
    public Dictionary<string, object> JourneySummary { get; set; } = new();
}

public class OriginVerification
{
    public string ProductId { get; set; } = string.Empty;
    public bool OriginVerified { get; set; }
    public string OriginLocation { get; set; } = string.Empty;
    public DateTime OriginDate { get; set; }
    public string OriginCertification { get; set; } = string.Empty;
    public double TrustScore { get; set; }
    public List<string> VerificationMethods { get; set; } = new();
    public Dictionary<string, object> OriginData { get; set; } = new();
}

public class QualityCheckRequest
{
    public string ProductId { get; set; } = string.Empty;
    public string CheckType { get; set; } = string.Empty;
    public string Inspector { get; set; } = string.Empty;
    public Dictionary<string, object> QualityMetrics { get; set; } = new();
    public List<string> Standards { get; set; } = new();
    public string Location { get; set; } = string.Empty;
}

public class QualityAssurance
{
    public string QualityId { get; set; } = string.Empty;
    public string ProductId { get; set; } = string.Empty;
    public string CheckType { get; set; } = string.Empty;
    public bool Passed { get; set; }
    public double QualityScore { get; set; }
    public DateTime CheckDate { get; set; }
    public string Inspector { get; set; } = string.Empty;
    public Dictionary<string, object> QualityMetrics { get; set; } = new();
    public List<string> Issues { get; set; } = new();
    public string TransactionHash { get; set; } = string.Empty;
}

public class SupplierCertification
{
    public string SupplierId { get; set; } = string.Empty;
    public bool IsValid { get; set; }
    public List<string> Certifications { get; set; } = new();
    public DateTime VerificationDate { get; set; }
    public DateTime ExpirationDate { get; set; }
    public string CertifyingAuthority { get; set; } = string.Empty;
    public double ComplianceScore { get; set; }
    public Dictionary<string, object> CertificationData { get; set; } = new();
}

public class SustainabilityRequest
{
    public string ProductId { get; set; } = string.Empty;
    public Dictionary<string, object> SustainabilityMetrics { get; set; } = new();
    public string ReportingStandard { get; set; } = string.Empty;
    public DateTime ReportingDate { get; set; }
}

public class SustainabilityMetrics
{
    public string ProductId { get; set; } = string.Empty;
    public double CarbonFootprint { get; set; }
    public double WaterUsage { get; set; }
    public double EnergyConsumption { get; set; }
    public double WasteGeneration { get; set; }
    public double SustainabilityScore { get; set; }
    public List<string> SustainabilityGoals { get; set; } = new();
    public DateTime RecordedAt { get; set; }
    public string TransactionHash { get; set; } = string.Empty;
}

public class ComplianceRequest
{
    public string EntityId { get; set; } = string.Empty;
    public string ComplianceType { get; set; } = string.Empty;
    public Dictionary<string, object> ComplianceData { get; set; } = new();
    public List<string> Regulations { get; set; } = new();
}

public class ComplianceRecord
{
    public string ComplianceId { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string ComplianceType { get; set; } = string.Empty;
    public bool IsCompliant { get; set; }
    public double ComplianceScore { get; set; }
    public DateTime AssessmentDate { get; set; }
    public List<string> Violations { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
    public string TransactionHash { get; set; } = string.Empty;
}

public class SupplyChainAnalytics
{
    public TimeSpan AnalysisPeriod { get; set; }
    public int TotalProducts { get; set; }
    public int TotalSteps { get; set; }
    public int TotalParticipants { get; set; }
    public double AverageQualityScore { get; set; }
    public double AverageSustainabilityScore { get; set; }
    public Dictionary<string, object> TrendAnalysis { get; set; } = new();
    public List<string> TopPerformers { get; set; } = new();
    public List<string> IssuesIdentified { get; set; } = new();
    public DateTime GeneratedAt { get; set; }
}

public class AnalyticsFilter
{
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public List<string> ProductIds { get; set; } = new();
    public List<string> ParticipantIds { get; set; } = new();
    public List<string> MetricTypes { get; set; } = new();
}

public class AlertConfiguration
{
    public string AlertType { get; set; } = string.Empty;
    public Dictionary<string, object> Conditions { get; set; } = new();
    public List<string> Recipients { get; set; } = new();
    public string NotificationMethod { get; set; } = string.Empty;
}

public class AlertResult
{
    public string AlertId { get; set; } = string.Empty;
    public bool AlertConfigured { get; set; }
    public string AlertType { get; set; } = string.Empty;
    public DateTime ConfiguredAt { get; set; }
    public List<string> MonitoredEntities { get; set; } = new();
    public Dictionary<string, object> AlertSettings { get; set; } = new();
}

public class IoTIntegrationRequest
{
    public string DeviceId { get; set; } = string.Empty;
    public string DeviceType { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public Dictionary<string, object> SensorData { get; set; } = new();
    public string ProductId { get; set; } = string.Empty;
}

public class IoTIntegrationResult
{
    public string IntegrationId { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string DeviceId { get; set; } = string.Empty;
    public DateTime IntegratedAt { get; set; }
    public Dictionary<string, object> IntegrationData { get; set; } = new();
    public string TransactionHash { get; set; } = string.Empty;
}

// Additional missing blockchain models
public class ContractFunctionCall
{
    public string ContractAddress { get; set; } = string.Empty;
    public string FunctionName { get; set; } = string.Empty;
    public List<object> Parameters { get; set; } = new();
    public string CallerAddress { get; set; } = string.Empty;
    public decimal GasLimit { get; set; } = 100000;
    public bool ReadOnly { get; set; } = false;
}

public class EventQuery
{
    public string ContractAddress { get; set; } = string.Empty;
    public string EventName { get; set; } = string.Empty;
    public Dictionary<string, object> FilterParameters { get; set; } = new();
    public long FromBlock { get; set; }
    public long ToBlock { get; set; }
}

public class ContractUpgradeRequest
{
    public string ContractAddress { get; set; } = string.Empty;
    public string NewImplementation { get; set; } = string.Empty;
    public Dictionary<string, object> UpgradeData { get; set; } = new();
    public string AdminAddress { get; set; } = string.Empty;
}

public class ContractUpgrade
{
    public string UpgradeId { get; set; } = string.Empty;
    public string ContractAddress { get; set; } = string.Empty;
    public string OldImplementation { get; set; } = string.Empty;
    public string NewImplementation { get; set; } = string.Empty;
    public string TransactionHash { get; set; } = string.Empty;
    public DateTime UpgradedAt { get; set; }
    public bool Success { get; set; }
}

public class ContractAudit
{
    public string AuditId { get; set; } = string.Empty;
    public string ContractAddress { get; set; } = string.Empty;
    public string AuditorAddress { get; set; } = string.Empty;
    public List<string> SecurityFindings { get; set; } = new();
    public string RiskLevel { get; set; } = string.Empty;
    public double SecurityScore { get; set; }
    public DateTime AuditDate { get; set; }
    public bool Passed { get; set; }
}

public class GasOptimizationRequest
{
    public string ContractAddress { get; set; } = string.Empty;
    public List<string> FunctionNames { get; set; } = new();
    public Dictionary<string, object> OptimizationParams { get; set; } = new();
}

public class GasOptimization
{
    public string OptimizationId { get; set; } = string.Empty;
    public string ContractAddress { get; set; } = string.Empty;
    public Dictionary<string, decimal> OriginalGasCosts { get; set; } = new();
    public Dictionary<string, decimal> OptimizedGasCosts { get; set; } = new();
    public double TotalSavings { get; set; }
    public List<string> OptimizationTechniques { get; set; } = new();
}

public class TemplateRequest
{
    public string TemplateName { get; set; } = string.Empty;
    public string TemplateType { get; set; } = string.Empty;
    public Dictionary<string, object> Parameters { get; set; } = new();
    public string RequesterAddress { get; set; } = string.Empty;
}

public class ContractTemplate
{
    public string TemplateId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string SourceCode { get; set; } = string.Empty;
    public List<string> RequiredParameters { get; set; } = new();
    public string Category { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class ContractValidationRequest
{
    public string SourceCode { get; set; } = string.Empty;
    public string CompilerVersion { get; set; } = string.Empty;
    public List<string> ValidationRules { get; set; } = new();
}

public class IdentityCreationRequest
{
    public string OwnerAddress { get; set; } = string.Empty;
    public Dictionary<string, object> Attributes { get; set; } = new();
    public List<string> ControllerAddresses { get; set; } = new();
    public string IdentityType { get; set; } = "Individual";
}

public class CredentialRequest
{
    public string IssuerDID { get; set; } = string.Empty;
    public string SubjectDID { get; set; } = string.Empty;
    public Dictionary<string, object> Claims { get; set; } = new();
    public DateTime ExpirationDate { get; set; }
    public string CredentialType { get; set; } = string.Empty;
}

public class CredentialVerification
{
    public string CredentialId { get; set; } = string.Empty;
    public bool IsValid { get; set; }
    public bool IsRevoked { get; set; }
    public double TrustScore { get; set; }
    public DateTime VerificationDate { get; set; }
    public string VerificationMethod { get; set; } = string.Empty;
    public List<string> ValidationResults { get; set; } = new();
}

public class AttributeRequest
{
    public string IdentityDID { get; set; } = string.Empty;
    public string AttributeName { get; set; } = string.Empty;
    public object AttributeValue { get; set; } = new();
    public bool IsPrivate { get; set; } = false;
    public string IssuerDID { get; set; } = string.Empty;
}

public class IdentityAttribute
{
    public string AttributeId { get; set; } = string.Empty;
    public string IdentityDID { get; set; } = string.Empty;
    public string AttributeName { get; set; } = string.Empty;
    public object AttributeValue { get; set; } = new();
    public bool IsPrivate { get; set; }
    public string IssuerDID { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
}

public class ConsentRequest
{
    public string DataSubjectDID { get; set; } = string.Empty;
    public string DataProcessorDID { get; set; } = string.Empty;
    public List<string> DataTypes { get; set; } = new();
    public string Purpose { get; set; } = string.Empty;
    public DateTime ExpirationDate { get; set; }
}

public class ConsentRecord
{
    public string ConsentId { get; set; } = string.Empty;
    public string DataSubjectDID { get; set; } = string.Empty;
    public string DataProcessorDID { get; set; } = string.Empty;
    public bool ConsentGiven { get; set; }
    public List<string> DataTypes { get; set; } = new();
    public string Purpose { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime ExpirationDate { get; set; }
    public bool IsRevoked { get; set; }
}

public class PrivacyRequest
{
    public string SubjectDID { get; set; } = string.Empty;
    public string RequestType { get; set; } = string.Empty; // Access, Portability, Deletion
    public List<string> DataCategories { get; set; } = new();
    public string RequesterDID { get; set; } = string.Empty;
}

public class PrivacyCompliance
{
    public string ComplianceId { get; set; } = string.Empty;
    public string SubjectDID { get; set; } = string.Empty;
    public string RequestType { get; set; } = string.Empty;
    public bool RequestFulfilled { get; set; }
    public DateTime ProcessedAt { get; set; }
    public List<string> ActionsToken { get; set; } = new();
    public string ProcessorDID { get; set; } = string.Empty;
}

public class RecoveryRequest
{
    public string IdentityDID { get; set; } = string.Empty;
    public string RecoveryMethod { get; set; } = string.Empty;
    public Dictionary<string, object> RecoveryData { get; set; } = new();
    public List<string> GuardianAddresses { get; set; } = new();
}

public class IdentityRecovery
{
    public string RecoveryId { get; set; } = string.Empty;
    public string IdentityDID { get; set; } = string.Empty;
    public string RecoveryMethod { get; set; } = string.Empty;
    public bool RecoverySuccessful { get; set; }
    public DateTime RecoveryDate { get; set; }
    public string NewControllerAddress { get; set; } = string.Empty;
    public List<string> GuardianSignatures { get; set; } = new();
}

public class BiometricRequest
{
    public string IdentityDID { get; set; } = string.Empty;
    public string BiometricType { get; set; } = string.Empty; // Fingerprint, Face, etc.
    public string BiometricHash { get; set; } = string.Empty;
    public Dictionary<string, object> BiometricMetadata { get; set; } = new();
}

public class BiometricRecord
{
    public string RecordId { get; set; } = string.Empty;
    public string IdentityDID { get; set; } = string.Empty;
    public string BiometricType { get; set; } = string.Empty;
    public string BiometricHash { get; set; } = string.Empty;
    public DateTime RecordedAt { get; set; }
    public bool IsActive { get; set; }
    public string TransactionHash { get; set; } = string.Empty;
}

public class ZKProofRequest
{
    public string ProofType { get; set; } = string.Empty;
    public Dictionary<string, object> PublicInputs { get; set; } = new();
    public Dictionary<string, object> PrivateInputs { get; set; } = new();
    public string CircuitHash { get; set; } = string.Empty;
}

public class ZeroKnowledgeProof
{
    public string ProofId { get; set; } = string.Empty;
    public string ProofType { get; set; } = string.Empty;
    public string Proof { get; set; } = string.Empty;
    public Dictionary<string, object> PublicInputs { get; set; } = new();
    public string VerificationKey { get; set; } = string.Empty;
    public bool IsValid { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class TokenCreationRequest
{
    public string TokenName { get; set; } = string.Empty;
    public string TokenSymbol { get; set; } = string.Empty;
    public decimal InitialSupply { get; set; }
    public int Decimals { get; set; } = 18;
    public string TokenType { get; set; } = "ERC20";
    public string CreatorAddress { get; set; } = string.Empty;
    public Dictionary<string, object> TokenProperties { get; set; } = new();
}

public class MintingRequest
{
    public string TokenContract { get; set; } = string.Empty;
    public string RecipientAddress { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? TokenId { get; set; } // For NFTs
    public Dictionary<string, object> Metadata { get; set; } = new();
    public string MinterAddress { get; set; } = string.Empty;
}

public class BurningRequest
{
    public string TokenContract { get; set; } = string.Empty;
    public string OwnerAddress { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? TokenId { get; set; } // For NFTs
    public string BurnerAddress { get; set; } = string.Empty;
}

public class TokenBurning
{
    public string BurningId { get; set; } = string.Empty;
    public string TokenContract { get; set; } = string.Empty;
    public string OwnerAddress { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string TransactionHash { get; set; } = string.Empty;
    public DateTime BurnedAt { get; set; }
    public bool Success { get; set; }
}

public class TransferRequest
{
    public string TokenContract { get; set; } = string.Empty;
    public string FromAddress { get; set; } = string.Empty;
    public string ToAddress { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? TokenId { get; set; } // For NFTs
    public Dictionary<string, object> TransferData { get; set; } = new();
}

public class TokenTransfer
{
    public string TransferId { get; set; } = string.Empty;
    public string TokenContract { get; set; } = string.Empty;
    public string FromAddress { get; set; } = string.Empty;
    public string ToAddress { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string TransactionHash { get; set; } = string.Empty;
    public DateTime TransferredAt { get; set; }
    public bool Success { get; set; }
}

public class TokenBalance
{
    public string Address { get; set; } = string.Empty;
    public string TokenContract { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public DateTime LastUpdated { get; set; }
    public List<string> OwnedTokenIds { get; set; } = new(); // For NFTs
}