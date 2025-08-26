using VHouse.Classes;

namespace VHouse.Interfaces
{
    /// <summary>
    /// Service interface for comprehensive security operations and threat protection.
    /// </summary>
    public interface ISecurityService
    {
        /// <summary>
        /// Validates API requests for security threats and rate limiting.
        /// </summary>
        Task<SecurityValidationResult> ValidateRequestAsync(SecurityRequestContext context);

        /// <summary>
        /// Encrypts sensitive data using AES encryption.
        /// </summary>
        Task<string> EncryptSensitiveDataAsync(string data);

        /// <summary>
        /// Decrypts sensitive data using AES encryption.
        /// </summary>
        Task<string> DecryptSensitiveDataAsync(string encryptedData);

        /// <summary>
        /// Generates secure tokens for API authentication.
        /// </summary>
        Task<string> GenerateSecureTokenAsync(TokenType tokenType, string userId, TimeSpan expiration);

        /// <summary>
        /// Validates and decodes secure tokens.
        /// </summary>
        Task<TokenValidationResult> ValidateTokenAsync(string token);

        /// <summary>
        /// Logs security events for auditing and monitoring.
        /// </summary>
        Task LogSecurityEventAsync(SecurityEvent securityEvent);

        /// <summary>
        /// Checks if an IP address is blocked or rate limited.
        /// </summary>
        Task<bool> IsIpBlockedAsync(string ipAddress);

        /// <summary>
        /// Blocks an IP address for security violations.
        /// </summary>
        Task BlockIpAddressAsync(string ipAddress, string reason, TimeSpan? duration = null);

        /// <summary>
        /// Validates password strength and complexity.
        /// </summary>
        Task<PasswordValidationResult> ValidatePasswordAsync(string password);

        /// <summary>
        /// Hashes passwords using bcrypt with salt.
        /// </summary>
        Task<string> HashPasswordAsync(string password);

        /// <summary>
        /// Verifies password against hash.
        /// </summary>
        Task<bool> VerifyPasswordAsync(string password, string hash);

        /// <summary>
        /// Gets security metrics and threat intelligence.
        /// </summary>
        Task<SecurityMetrics> GetSecurityMetricsAsync(DateTime? fromDate = null, DateTime? toDate = null);

        /// <summary>
        /// Performs security scan on user input for SQL injection, XSS, etc.
        /// </summary>
        Task<SecurityScanResult> ScanUserInputAsync(string input, ScanType scanType);
    }

    /// <summary>
    /// Security request context for validation.
    /// </summary>
    public class SecurityRequestContext
    {
        public string IpAddress { get; set; } = string.Empty;
        public string UserAgent { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string Endpoint { get; set; } = string.Empty;
        public string HttpMethod { get; set; } = string.Empty;
        public Dictionary<string, string> Headers { get; set; } = new();
        public string RequestBody { get; set; } = string.Empty;
        public DateTime RequestTime { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Result of security validation.
    /// </summary>
    public class SecurityValidationResult
    {
        public bool IsValid { get; set; }
        public bool IsRateLimited { get; set; }
        public bool IsBlocked { get; set; }
        public List<SecurityThreat> Threats { get; set; } = new();
        public string Message { get; set; } = string.Empty;
        public TimeSpan? RetryAfter { get; set; }
    }

    /// <summary>
    /// Security threat information.
    /// </summary>
    public class SecurityThreat
    {
        public ThreatType Type { get; set; }
        public ThreatSeverity Severity { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public bool IsBlocked { get; set; }
        public DateTime DetectedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Types of security threats.
    /// </summary>
    public enum ThreatType
    {
        SqlInjection,
        CrossSiteScripting,
        CommandInjection,
        PathTraversal,
        Ddos,
        BruteForce,
        Malware,
        SuspiciousActivity,
        RateLimitExceeded
    }

    /// <summary>
    /// Severity levels for threats.
    /// </summary>
    public enum ThreatSeverity
    {
        Low,
        Medium,
        High,
        Critical
    }

    /// <summary>
    /// Token types for generation.
    /// </summary>
    public enum TokenType
    {
        AccessToken,
        RefreshToken,
        ApiKey,
        PasswordReset,
        EmailVerification
    }

    /// <summary>
    /// Result of token validation.
    /// </summary>
    public class TokenValidationResult
    {
        public bool IsValid { get; set; }
        public bool IsExpired { get; set; }
        public string UserId { get; set; } = string.Empty;
        public TokenType TokenType { get; set; }
        public DateTime ExpiresAt { get; set; }
        public Dictionary<string, object> Claims { get; set; } = new();
        public string ErrorMessage { get; set; } = string.Empty;
    }

    /// <summary>
    /// Security event for logging.
    /// </summary>
    public class SecurityEvent
    {
        public string EventId { get; set; } = Guid.NewGuid().ToString();
        public SecurityEventType EventType { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public string UserAgent { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public ThreatSeverity Severity { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    /// <summary>
    /// Types of security events.
    /// </summary>
    public enum SecurityEventType
    {
        Login,
        Logout,
        FailedLogin,
        PasswordChange,
        AccountLocked,
        ThreatDetected,
        IpBlocked,
        SuspiciousActivity,
        DataAccess,
        PrivilegeEscalation
    }

    /// <summary>
    /// Password validation result.
    /// </summary>
    public class PasswordValidationResult
    {
        public bool IsValid { get; set; }
        public int Score { get; set; } // 0-100
        public List<string> Violations { get; set; } = new();
        public List<string> Suggestions { get; set; } = new();
        public PasswordStrength Strength { get; set; }
    }

    /// <summary>
    /// Password strength levels.
    /// </summary>
    public enum PasswordStrength
    {
        VeryWeak,
        Weak,
        Fair,
        Good,
        Strong,
        VeryStrong
    }

    /// <summary>
    /// Security metrics and statistics.
    /// </summary>
    public class SecurityMetrics
    {
        public int TotalThreats { get; set; }
        public int BlockedRequests { get; set; }
        public int FailedLogins { get; set; }
        public int BlockedIpAddresses { get; set; }
        public Dictionary<ThreatType, int> ThreatsByType { get; set; } = new();
        public Dictionary<string, int> TopAttackingIpAddresses { get; set; } = new();
        public List<SecurityTrend> ThreatTrends { get; set; } = new();
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public decimal ThreatGrowthRate { get; set; }
        public string SecurityStatus { get; set; } = "Normal";
    }

    /// <summary>
    /// Security trend data.
    /// </summary>
    public class SecurityTrend
    {
        public DateTime Date { get; set; }
        public int ThreatCount { get; set; }
        public int BlockedCount { get; set; }
        public ThreatType MostCommonThreat { get; set; }
    }

    /// <summary>
    /// Result of input security scanning.
    /// </summary>
    public class SecurityScanResult
    {
        public bool IsSafe { get; set; }
        public List<SecurityThreat> ThreatsFound { get; set; } = new();
        public string SanitizedInput { get; set; } = string.Empty;
        public ScanType ScanType { get; set; }
        public int ConfidenceScore { get; set; } // 0-100
    }

    /// <summary>
    /// Types of security scans.
    /// </summary>
    public enum ScanType
    {
        SqlInjection,
        XssAttack,
        CommandInjection,
        PathTraversal,
        EmailValidation,
        UrlValidation,
        General
    }
}