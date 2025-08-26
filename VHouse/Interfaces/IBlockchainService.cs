using VHouse.Classes;

namespace VHouse.Interfaces;

public interface IBlockchainService
{
    Task<TransactionResult> RecordSupplyChainEventAsync(SupplyChainEvent eventData);
    Task<SmartContractResult> ExecuteSmartContractAsync(ContractExecution execution);
    Task<VerificationResult> VerifyProductAuthenticityAsync(string productId);
    Task<BlockchainWallet> CreateWalletAsync(WalletCreationRequest request);
    Task<TokenTransaction> TransferTokensAsync(TokenTransferRequest request);
    Task<SmartContract> DeploySmartContractAsync(SmartContractDeployment deployment);
    Task<BlockchainTransaction> GetTransactionAsync(string transactionHash);
    Task<BlockchainBlock> GetBlockAsync(string blockHash);
    Task<ConsensusResult> ParticipateInConsensusAsync(ConsensusRequest request);
    Task<NetworkStatus> GetNetworkStatusAsync();
    Task<GasEstimation> EstimateGasAsync(GasEstimationRequest request);
    Task<ContractInteraction> InteractWithContractAsync(ContractInteractionRequest request);
    Task<BlockchainAudit> AuditTransactionHistoryAsync(AuditRequest request);
    Task<CryptographicProof> GenerateProofAsync(ProofRequest request);
    Task<IdentityVerification> VerifyIdentityAsync(IdentityVerificationRequest request);
}

public interface ISupplyChainBlockchainService
{
    Task<SupplyChainRecord> RecordSupplyChainStepAsync(SupplyChainStepRequest request);
    Task<ProductJourney> TraceProductJourneyAsync(string productId);
    Task<OriginVerification> VerifyProductOriginAsync(string productId);
    Task<QualityAssurance> RecordQualityCheckAsync(QualityCheckRequest request);
    Task<SupplierCertification> VerifySupplierCertificationAsync(string supplierId);
    Task<SustainabilityMetrics> TrackSustainabilityAsync(SustainabilityRequest request);
    Task<ComplianceRecord> RecordComplianceAsync(ComplianceRequest request);
    Task<SupplyChainAnalytics> GetSupplyChainAnalyticsAsync(AnalyticsFilter filter);
    Task<AlertResult> SetupSupplyChainAlertsAsync(AlertConfiguration config);
    Task<IntegrationResult> IntegrateWithIoTAsync(IoTIntegrationRequest request);
}

public interface ISmartContractService
{
    Task<SmartContract> CreateContractAsync(ContractCreationRequest request);
    Task<ContractDeployment> DeployContractAsync(string contractId, DeploymentConfig config);
    Task<ExecutionResult> ExecuteContractFunctionAsync(ContractFunctionCall call);
    Task<ContractState> GetContractStateAsync(string contractAddress);
    Task<ContractEvent> QueryContractEventsAsync(EventQuery query);
    Task<ContractUpgrade> UpgradeContractAsync(ContractUpgradeRequest request);
    Task<ContractAudit> AuditContractAsync(string contractAddress);
    Task<GasOptimization> OptimizeContractGasAsync(GasOptimizationRequest request);
    Task<ContractTemplate> CreateContractTemplateAsync(TemplateRequest request);
    Task<ValidationResult> ValidateContractAsync(ContractValidationRequest request);
}

public interface IDigitalIdentityService
{
    Task<DigitalIdentity> CreateDigitalIdentityAsync(IdentityCreationRequest request);
    Task<IdentityVerification> VerifyDigitalIdentityAsync(string identityId);
    Task<CredentialIssuance> IssueCredentialAsync(CredentialRequest request);
    Task<CredentialVerification> VerifyCredentialAsync(string credentialId);
    Task<IdentityAttribute> AddIdentityAttributeAsync(AttributeRequest request);
    Task<ConsentRecord> RecordConsentAsync(ConsentRequest request);
    Task<PrivacyCompliance> EnsurePrivacyComplianceAsync(PrivacyRequest request);
    Task<IdentityRecovery> RecoverIdentityAsync(RecoveryRequest request);
    Task<BiometricRecord> RegisterBiometricAsync(BiometricRequest request);
    Task<ZeroKnowledgeProof> GenerateZKProofAsync(ZKProofRequest request);
}

public interface ITokenizationService
{
    Task<TokenCreation> CreateTokenAsync(TokenCreationRequest request);
    Task<TokenMinting> MintTokensAsync(MintingRequest request);
    Task<TokenBurning> BurnTokensAsync(BurningRequest request);
    Task<TokenTransfer> TransferTokensAsync(TransferRequest request);
    Task<TokenBalance> GetTokenBalanceAsync(string walletAddress, string tokenContract);
    Task<TokenMetadata> GetTokenMetadataAsync(string tokenId);
    Task<TokenHistory> GetTokenHistoryAsync(string tokenId);
    Task<StakingResult> StakeTokensAsync(StakingRequest request);
    Task<RewardDistribution> DistributeRewardsAsync(RewardRequest request);
    Task<GovernanceVote> VoteOnProposalAsync(VotingRequest request);
}