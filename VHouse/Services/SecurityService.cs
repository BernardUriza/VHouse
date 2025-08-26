using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using VHouse.Interfaces;

namespace VHouse.Services
{
    /// <summary>
    /// Comprehensive security service for threat protection and data security.
    /// </summary>
    public class SecurityService : ISecurityService
    {
        private readonly ILogger<SecurityService> _logger;
        private readonly ICachingService _cachingService;
        private readonly IConfiguration _configuration;
        private readonly Dictionary<string, DateTime> _rateLimitTracker = new();
        private readonly Dictionary<string, int> _requestCounts = new();
        private readonly HashSet<string> _blockedIps = new();
        
        // Security patterns for threat detection
        private static readonly Dictionary<ThreatType, Regex[]> ThreatPatterns = new()
        {
            [ThreatType.SqlInjection] = new[]
            {
                new Regex(@"('|(\\')|(;)|(\\;))", RegexOptions.IgnoreCase),
                new Regex(@"(\b(SELECT|INSERT|UPDATE|DELETE|DROP|CREATE|ALTER|EXEC|UNION|SCRIPT)\b)", RegexOptions.IgnoreCase),
                new Regex(@"(\b(OR|AND)\s+\w+\s*=\s*\w+)", RegexOptions.IgnoreCase)
            },
            [ThreatType.CrossSiteScripting] = new[]
            {
                new Regex(@"<script[^>]*>.*?</script>", RegexOptions.IgnoreCase | RegexOptions.Singleline),
                new Regex(@"javascript\s*:", RegexOptions.IgnoreCase),
                new Regex(@"on\w+\s*=", RegexOptions.IgnoreCase)
            },
            [ThreatType.CommandInjection] = new[]
            {
                new Regex(@"[;&|`]", RegexOptions.None),
                new Regex(@"\b(cmd|powershell|bash|sh)\b", RegexOptions.IgnoreCase)
            },
            [ThreatType.PathTraversal] = new[]
            {
                new Regex(@"\.\./", RegexOptions.None),
                new Regex(@"\\\.\\", RegexOptions.None),
                new Regex(@"\.\.\\", RegexOptions.None)
            }
        };

        public SecurityService(
            ILogger<SecurityService> logger,
            ICachingService cachingService,
            IConfiguration configuration)
        {
            _logger = logger;
            _cachingService = cachingService;
            _configuration = configuration;
        }

        /// <summary>
        /// Validates API requests for security threats and rate limiting.
        /// </summary>
        public async Task<SecurityValidationResult> ValidateRequestAsync(SecurityRequestContext context)
        {
            var result = new SecurityValidationResult { IsValid = true };

            try
            {
                // Check if IP is blocked
                if (await IsIpBlockedAsync(context.IpAddress))
                {
                    result.IsValid = false;
                    result.IsBlocked = true;
                    result.Message = "IP address is blocked due to security violations";
                    
                    await LogSecurityEventAsync(new SecurityEvent
                    {
                        EventType = SecurityEventType.SuspiciousActivity,
                        IpAddress = context.IpAddress,
                        Description = "Blocked IP attempted access",
                        Severity = ThreatSeverity.High
                    });

                    return result;
                }

                // Rate limiting check
                if (await IsRateLimitExceededAsync(context.IpAddress, context.Endpoint))
                {
                    result.IsValid = false;
                    result.IsRateLimited = true;
                    result.RetryAfter = TimeSpan.FromMinutes(1);
                    result.Message = "Rate limit exceeded. Please try again later.";

                    result.Threats.Add(new SecurityThreat
                    {
                        Type = ThreatType.RateLimitExceeded,
                        Severity = ThreatSeverity.Medium,
                        Description = "Rate limit exceeded",
                        Location = context.Endpoint
                    });

                    return result;
                }

                // Scan request body for threats
                if (!string.IsNullOrEmpty(context.RequestBody))
                {
                    var scanResult = await ScanUserInputAsync(context.RequestBody, ScanType.General);
                    if (!scanResult.IsSafe)
                    {
                        result.IsValid = false;
                        result.Threats.AddRange(scanResult.ThreatsFound);
                        result.Message = "Malicious content detected in request";
                    }
                }

                // Check for suspicious user agent patterns
                if (IsSuspiciousUserAgent(context.UserAgent))
                {
                    result.Threats.Add(new SecurityThreat
                    {
                        Type = ThreatType.SuspiciousActivity,
                        Severity = ThreatSeverity.Medium,
                        Description = "Suspicious user agent detected",
                        Location = "User-Agent header"
                    });
                }

                // Log successful validation
                if (result.IsValid && result.Threats.Any())
                {
                    await LogSecurityEventAsync(new SecurityEvent
                    {
                        EventType = SecurityEventType.ThreatDetected,
                        IpAddress = context.IpAddress,
                        UserAgent = context.UserAgent,
                        Description = "Security threats detected but request allowed",
                        Severity = ThreatSeverity.Low,
                        Details = JsonSerializer.Serialize(result.Threats)
                    });
                }

                _logger.LogInformation("Security validation completed for {IpAddress} on {Endpoint}. Valid: {IsValid}, Threats: {ThreatCount}",
                    context.IpAddress, context.Endpoint, result.IsValid, result.Threats.Count);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during security validation for {IpAddress}", context.IpAddress);
                
                // Fail securely - block request on error
                result.IsValid = false;
                result.Message = "Security validation failed";
                return result;
            }
        }

        /// <summary>
        /// Encrypts sensitive data using AES encryption.
        /// </summary>
        public async Task<string> EncryptSensitiveDataAsync(string data)
        {
            try
            {
                if (string.IsNullOrEmpty(data))
                    return string.Empty;

                var key = GetEncryptionKey();
                using var aes = Aes.Create();
                aes.Key = key;
                aes.GenerateIV();

                using var encryptor = aes.CreateEncryptor();
                var dataBytes = Encoding.UTF8.GetBytes(data);
                var encryptedBytes = encryptor.TransformFinalBlock(dataBytes, 0, dataBytes.Length);

                // Combine IV and encrypted data
                var result = new byte[aes.IV.Length + encryptedBytes.Length];
                Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
                Buffer.BlockCopy(encryptedBytes, 0, result, aes.IV.Length, encryptedBytes.Length);

                return Convert.ToBase64String(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error encrypting sensitive data");
                throw;
            }
        }

        /// <summary>
        /// Decrypts sensitive data using AES encryption.
        /// </summary>
        public async Task<string> DecryptSensitiveDataAsync(string encryptedData)
        {
            try
            {
                if (string.IsNullOrEmpty(encryptedData))
                    return string.Empty;

                var key = GetEncryptionKey();
                var fullCipher = Convert.FromBase64String(encryptedData);

                using var aes = Aes.Create();
                aes.Key = key;

                // Extract IV and encrypted data
                var iv = new byte[aes.BlockSize / 8];
                var cipher = new byte[fullCipher.Length - iv.Length];

                Buffer.BlockCopy(fullCipher, 0, iv, 0, iv.Length);
                Buffer.BlockCopy(fullCipher, iv.Length, cipher, 0, cipher.Length);

                aes.IV = iv;

                using var decryptor = aes.CreateDecryptor();
                var decryptedBytes = decryptor.TransformFinalBlock(cipher, 0, cipher.Length);

                return Encoding.UTF8.GetString(decryptedBytes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error decrypting sensitive data");
                throw;
            }
        }

        /// <summary>
        /// Generates secure tokens for API authentication.
        /// </summary>
        public async Task<string> GenerateSecureTokenAsync(TokenType tokenType, string userId, TimeSpan expiration)
        {
            try
            {
                var tokenData = new
                {
                    TokenType = tokenType.ToString(),
                    UserId = userId,
                    IssuedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                    ExpiresAt = DateTimeOffset.UtcNow.Add(expiration).ToUnixTimeSeconds(),
                    Nonce = Guid.NewGuid().ToString()
                };

                var tokenJson = JsonSerializer.Serialize(tokenData);
                var encryptedToken = await EncryptSensitiveDataAsync(tokenJson);

                _logger.LogInformation("Generated {TokenType} token for user {UserId} with expiration {Expiration}",
                    tokenType, userId, expiration);

                return encryptedToken;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating secure token for user {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Validates and decodes secure tokens.
        /// </summary>
        public async Task<TokenValidationResult> ValidateTokenAsync(string token)
        {
            var result = new TokenValidationResult();

            try
            {
                if (string.IsNullOrEmpty(token))
                {
                    result.ErrorMessage = "Token is empty";
                    return result;
                }

                var decryptedToken = await DecryptSensitiveDataAsync(token);
                var tokenData = JsonSerializer.Deserialize<Dictionary<string, object>>(decryptedToken);

                if (tokenData == null)
                {
                    result.ErrorMessage = "Invalid token format";
                    return result;
                }

                // Extract token information
                result.UserId = tokenData.GetValueOrDefault("UserId")?.ToString() ?? "";
                var tokenTypeStr = tokenData.GetValueOrDefault("TokenType")?.ToString() ?? "";
                Enum.TryParse<TokenType>(tokenTypeStr, out var tokenType);
                result.TokenType = tokenType;

                var expiresAtObj = tokenData.GetValueOrDefault("ExpiresAt");
                if (expiresAtObj != null && long.TryParse(expiresAtObj.ToString(), out var expiresAt))
                {
                    result.ExpiresAt = DateTimeOffset.FromUnixTimeSeconds(expiresAt).DateTime;
                    result.IsExpired = DateTime.UtcNow > result.ExpiresAt;
                }

                result.IsValid = !result.IsExpired;
                result.Claims = tokenData;

                if (result.IsExpired)
                {
                    result.ErrorMessage = "Token has expired";
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating token");
                result.ErrorMessage = "Token validation failed";
                return result;
            }
        }

        /// <summary>
        /// Logs security events for auditing and monitoring.
        /// </summary>
        public async Task LogSecurityEventAsync(SecurityEvent securityEvent)
        {
            try
            {
                // Log to structured logger
                _logger.LogWarning("Security Event: {EventType} | Severity: {Severity} | IP: {IpAddress} | User: {UserId} | Description: {Description}",
                    securityEvent.EventType, securityEvent.Severity, securityEvent.IpAddress, 
                    securityEvent.UserId, securityEvent.Description);

                // Store in cache for recent events tracking
                var cacheKey = $"security_events:{DateTime.UtcNow:yyyyMMdd}";
                var events = await _cachingService.GetAsync<List<SecurityEvent>>(cacheKey) ?? new List<SecurityEvent>();
                events.Add(securityEvent);

                // Keep only last 1000 events per day
                if (events.Count > 1000)
                {
                    events = events.TakeLast(1000).ToList();
                }

                await _cachingService.SetAsync(cacheKey, events, TimeSpan.FromDays(7));

                // In production, you would also send to SIEM, security monitoring tools, etc.
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging security event");
            }
        }

        /// <summary>
        /// Checks if an IP address is blocked or rate limited.
        /// </summary>
        public async Task<bool> IsIpBlockedAsync(string ipAddress)
        {
            try
            {
                var cacheKey = $"blocked_ip:{ipAddress}";
                var isBlocked = await _cachingService.ExistsAsync(cacheKey);
                
                return isBlocked || _blockedIps.Contains(ipAddress);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if IP {IpAddress} is blocked", ipAddress);
                return false;
            }
        }

        /// <summary>
        /// Blocks an IP address for security violations.
        /// </summary>
        public async Task BlockIpAddressAsync(string ipAddress, string reason, TimeSpan? duration = null)
        {
            try
            {
                duration ??= TimeSpan.FromHours(24); // Default 24 hour block

                var cacheKey = $"blocked_ip:{ipAddress}";
                await _cachingService.SetAsync(cacheKey, reason, duration.Value);
                
                _blockedIps.Add(ipAddress);

                await LogSecurityEventAsync(new SecurityEvent
                {
                    EventType = SecurityEventType.IpBlocked,
                    IpAddress = ipAddress,
                    Description = $"IP blocked for: {reason}",
                    Severity = ThreatSeverity.High,
                    Details = JsonSerializer.Serialize(new { Duration = duration.Value.ToString(), Reason = reason })
                });

                _logger.LogWarning("Blocked IP address {IpAddress} for {Duration} due to: {Reason}",
                    ipAddress, duration.Value, reason);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error blocking IP address {IpAddress}", ipAddress);
            }
        }

        /// <summary>
        /// Validates password strength and complexity.
        /// </summary>
        public async Task<PasswordValidationResult> ValidatePasswordAsync(string password)
        {
            var result = new PasswordValidationResult();
            var score = 0;

            try
            {
                if (string.IsNullOrEmpty(password))
                {
                    result.Violations.Add("Password cannot be empty");
                    return result;
                }

                // Length check
                if (password.Length < 8)
                {
                    result.Violations.Add("Password must be at least 8 characters long");
                }
                else if (password.Length >= 12)
                {
                    score += 20;
                }
                else
                {
                    score += 10;
                }

                // Character diversity checks
                if (password.Any(char.IsLower))
                    score += 15;
                else
                    result.Violations.Add("Password must contain lowercase letters");

                if (password.Any(char.IsUpper))
                    score += 15;
                else
                    result.Violations.Add("Password must contain uppercase letters");

                if (password.Any(char.IsDigit))
                    score += 15;
                else
                    result.Violations.Add("Password must contain numbers");

                if (password.Any(c => "!@#$%^&*()_+-=[]{}|;':\",./<>?".Contains(c)))
                    score += 20;
                else
                    result.Violations.Add("Password must contain special characters");

                // Additional complexity
                if (password.Length >= 16)
                    score += 15;

                // Common password check (simplified)
                var commonPasswords = new[] { "password", "123456", "qwerty", "admin", "letmein" };
                if (commonPasswords.Any(common => password.ToLower().Contains(common.ToLower())))
                {
                    result.Violations.Add("Password contains common patterns");
                    score -= 20;
                }

                result.Score = Math.Max(0, Math.Min(100, score));
                result.IsValid = result.Violations.Count == 0;
                result.Strength = GetPasswordStrength(result.Score);

                // Generate suggestions
                if (!result.IsValid)
                {
                    result.Suggestions.Add("Use a mix of uppercase and lowercase letters");
                    result.Suggestions.Add("Include numbers and special characters");
                    result.Suggestions.Add("Make it at least 12 characters long");
                    result.Suggestions.Add("Avoid common words and patterns");
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating password");
                result.Violations.Add("Password validation failed");
                return result;
            }
        }

        /// <summary>
        /// Hashes passwords using bcrypt with salt.
        /// </summary>
        public async Task<string> HashPasswordAsync(string password)
        {
            try
            {
                // Generate a salt
                var salt = new byte[128 / 8];
                using (var rng = RandomNumberGenerator.Create())
                {
                    rng.GetNonZeroBytes(salt);
                }

                // Hash the password
                var hashedPassword = KeyDerivation.Pbkdf2(
                    password: password,
                    salt: salt,
                    prf: KeyDerivationPrf.HMACSHA256,
                    iterationCount: 10000,
                    numBytesRequested: 256 / 8);

                // Combine salt and hash
                var combined = new byte[salt.Length + hashedPassword.Length];
                Buffer.BlockCopy(salt, 0, combined, 0, salt.Length);
                Buffer.BlockCopy(hashedPassword, 0, combined, salt.Length, hashedPassword.Length);

                return Convert.ToBase64String(combined);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error hashing password");
                throw;
            }
        }

        /// <summary>
        /// Verifies password against hash.
        /// </summary>
        public async Task<bool> VerifyPasswordAsync(string password, string hash)
        {
            try
            {
                var combined = Convert.FromBase64String(hash);
                
                // Extract salt
                var salt = new byte[128 / 8];
                Buffer.BlockCopy(combined, 0, salt, 0, salt.Length);

                // Hash the provided password with the same salt
                var hashedPassword = KeyDerivation.Pbkdf2(
                    password: password,
                    salt: salt,
                    prf: KeyDerivationPrf.HMACSHA256,
                    iterationCount: 10000,
                    numBytesRequested: 256 / 8);

                // Extract stored hash
                var storedHash = new byte[256 / 8];
                Buffer.BlockCopy(combined, salt.Length, storedHash, 0, storedHash.Length);

                // Compare hashes
                return hashedPassword.SequenceEqual(storedHash);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying password");
                return false;
            }
        }

        /// <summary>
        /// Gets security metrics and threat intelligence.
        /// </summary>
        public async Task<SecurityMetrics> GetSecurityMetricsAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                fromDate ??= DateTime.UtcNow.AddDays(-7);
                toDate ??= DateTime.UtcNow;

                var metrics = new SecurityMetrics
                {
                    PeriodStart = fromDate.Value,
                    PeriodEnd = toDate.Value
                };

                // Aggregate security events from cache
                var days = (int)(toDate.Value - fromDate.Value).TotalDays + 1;
                var allEvents = new List<SecurityEvent>();

                for (int i = 0; i < days; i++)
                {
                    var date = fromDate.Value.AddDays(i);
                    var cacheKey = $"security_events:{date:yyyyMMdd}";
                    var dayEvents = await _cachingService.GetAsync<List<SecurityEvent>>(cacheKey) ?? new List<SecurityEvent>();
                    allEvents.AddRange(dayEvents.Where(e => e.Timestamp >= fromDate && e.Timestamp <= toDate));
                }

                // Calculate metrics
                metrics.TotalThreats = allEvents.Count(e => e.EventType == SecurityEventType.ThreatDetected);
                metrics.BlockedRequests = allEvents.Count(e => e.EventType == SecurityEventType.SuspiciousActivity);
                metrics.FailedLogins = allEvents.Count(e => e.EventType == SecurityEventType.FailedLogin);
                metrics.BlockedIpAddresses = allEvents.Count(e => e.EventType == SecurityEventType.IpBlocked);

                // Threat trends
                metrics.ThreatTrends = allEvents
                    .GroupBy(e => e.Timestamp.Date)
                    .Select(g => new SecurityTrend
                    {
                        Date = g.Key,
                        ThreatCount = g.Count(e => e.EventType == SecurityEventType.ThreatDetected),
                        BlockedCount = g.Count(e => e.EventType == SecurityEventType.IpBlocked)
                    })
                    .OrderBy(t => t.Date)
                    .ToList();

                // Security status
                metrics.SecurityStatus = metrics.TotalThreats > 100 ? "High Alert" :
                                       metrics.TotalThreats > 50 ? "Elevated" : "Normal";

                _logger.LogInformation("Retrieved security metrics: {TotalThreats} threats, {BlockedRequests} blocked requests from {FromDate} to {ToDate}",
                    metrics.TotalThreats, metrics.BlockedRequests, fromDate, toDate);

                return metrics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving security metrics");
                return new SecurityMetrics();
            }
        }

        /// <summary>
        /// Performs security scan on user input for SQL injection, XSS, etc.
        /// </summary>
        public async Task<SecurityScanResult> ScanUserInputAsync(string input, ScanType scanType)
        {
            var result = new SecurityScanResult
            {
                ScanType = scanType,
                IsSafe = true,
                SanitizedInput = input,
                ConfidenceScore = 100
            };

            try
            {
                if (string.IsNullOrEmpty(input))
                    return result;

                var threatsToCheck = scanType == ScanType.General 
                    ? ThreatPatterns.Keys 
                    : GetThreatTypesForScan(scanType);

                foreach (var threatType in threatsToCheck)
                {
                    if (ThreatPatterns.TryGetValue(threatType, out var patterns))
                    {
                        foreach (var pattern in patterns)
                        {
                            var matches = pattern.Matches(input);
                            if (matches.Count > 0)
                            {
                                result.IsSafe = false;
                                result.ConfidenceScore = Math.Max(0, result.ConfidenceScore - (matches.Count * 15));

                                result.ThreatsFound.Add(new SecurityThreat
                                {
                                    Type = threatType,
                                    Severity = GetThreatSeverity(threatType, matches.Count),
                                    Description = $"{threatType} pattern detected: {matches.Count} matches",
                                    Location = $"Input at positions: {string.Join(", ", matches.Cast<Match>().Select(m => m.Index))}"
                                });

                                // Sanitize input by removing/escaping dangerous patterns
                                result.SanitizedInput = pattern.Replace(result.SanitizedInput, "");
                            }
                        }
                    }
                }

                if (!result.IsSafe)
                {
                    _logger.LogWarning("Security scan detected {ThreatCount} threats in input of type {ScanType}",
                        result.ThreatsFound.Count, scanType);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during security scan");
                result.IsSafe = false;
                result.ConfidenceScore = 0;
                return result;
            }
        }

        #region Private Helper Methods

        private byte[] GetEncryptionKey()
        {
            var keyString = _configuration["Security:EncryptionKey"] ?? "VHouse-Default-Key-Change-In-Production-2024";
            
            // Derive a 256-bit key from the configuration string
            using var rfc2898 = new Rfc2898DeriveBytes(keyString, Encoding.UTF8.GetBytes("VHouseSalt"), 10000, HashAlgorithmName.SHA256);
            return rfc2898.GetBytes(32); // 256 bits
        }

        private async Task<bool> IsRateLimitExceededAsync(string ipAddress, string endpoint)
        {
            var key = $"{ipAddress}:{endpoint}";
            var currentTime = DateTime.UtcNow;
            
            // Simple rate limiting: 100 requests per minute per IP per endpoint
            lock (_rateLimitTracker)
            {
                if (_rateLimitTracker.TryGetValue(key, out var lastReset))
                {
                    if (currentTime - lastReset > TimeSpan.FromMinutes(1))
                    {
                        _requestCounts[key] = 1;
                        _rateLimitTracker[key] = currentTime;
                        return false;
                    }
                    else
                    {
                        _requestCounts.TryGetValue(key, out var count);
                        _requestCounts[key] = count + 1;
                        return _requestCounts[key] > 100;
                    }
                }
                else
                {
                    _rateLimitTracker[key] = currentTime;
                    _requestCounts[key] = 1;
                    return false;
                }
            }
        }

        private static bool IsSuspiciousUserAgent(string userAgent)
        {
            if (string.IsNullOrEmpty(userAgent))
                return true;

            var suspiciousPatterns = new[]
            {
                "bot", "crawler", "spider", "scraper", "curl", "wget",
                "python", "java", "php", "perl", "ruby"
            };

            return suspiciousPatterns.Any(pattern => 
                userAgent.ToLower().Contains(pattern.ToLower()));
        }

        private static PasswordStrength GetPasswordStrength(int score)
        {
            return score switch
            {
                >= 80 => PasswordStrength.VeryStrong,
                >= 70 => PasswordStrength.Strong,
                >= 60 => PasswordStrength.Good,
                >= 40 => PasswordStrength.Fair,
                >= 20 => PasswordStrength.Weak,
                _ => PasswordStrength.VeryWeak
            };
        }

        private static IEnumerable<ThreatType> GetThreatTypesForScan(ScanType scanType)
        {
            return scanType switch
            {
                ScanType.SqlInjection => new[] { ThreatType.SqlInjection },
                ScanType.XssAttack => new[] { ThreatType.CrossSiteScripting },
                ScanType.CommandInjection => new[] { ThreatType.CommandInjection },
                ScanType.PathTraversal => new[] { ThreatType.PathTraversal },
                _ => ThreatPatterns.Keys
            };
        }

        private static ThreatSeverity GetThreatSeverity(ThreatType threatType, int matchCount)
        {
            var baseSeverity = threatType switch
            {
                ThreatType.SqlInjection => ThreatSeverity.Critical,
                ThreatType.CrossSiteScripting => ThreatSeverity.High,
                ThreatType.CommandInjection => ThreatSeverity.Critical,
                ThreatType.PathTraversal => ThreatSeverity.High,
                _ => ThreatSeverity.Medium
            };

            // Increase severity with more matches
            if (matchCount > 3)
                return ThreatSeverity.Critical;
            if (matchCount > 1 && baseSeverity < ThreatSeverity.High)
                return ThreatSeverity.High;

            return baseSeverity;
        }

        #endregion
    }
}