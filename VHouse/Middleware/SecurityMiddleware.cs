using Microsoft.AspNetCore.Http;
using System.Net;
using System.Text.Json;
using VHouse.Interfaces;

namespace VHouse.Middleware
{
    /// <summary>
    /// Security middleware for request validation, threat detection, and IP blocking.
    /// </summary>
    public class SecurityMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<SecurityMiddleware> _logger;
        private readonly IServiceProvider _serviceProvider;

        public SecurityMiddleware(
            RequestDelegate next,
            ILogger<SecurityMiddleware> logger,
            IServiceProvider serviceProvider)
        {
            _next = next;
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // Skip security checks for health checks and static files
                if (ShouldSkipSecurityCheck(context.Request.Path))
                {
                    await _next(context);
                    return;
                }

                using var scope = _serviceProvider.CreateScope();
                var securityService = scope.ServiceProvider.GetRequiredService<ISecurityService>();

                // Build security context
                var securityContext = await BuildSecurityContextAsync(context);

                // Validate request security
                var validationResult = await securityService.ValidateRequestAsync(securityContext);

                // Handle security validation results
                if (!validationResult.IsValid)
                {
                    await HandleSecurityViolationAsync(context, validationResult, securityService);
                    return;
                }

                // Add security headers
                AddSecurityHeaders(context.Response);

                // Log successful request if threats were detected but allowed
                if (validationResult.Threats.Any())
                {
                    _logger.LogWarning("Security threats detected but request allowed for {IpAddress} on {Path}: {ThreatCount} threats",
                        GetClientIpAddress(context), context.Request.Path, validationResult.Threats.Count);
                }

                // Continue with the request pipeline
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in security middleware for {Path}", context.Request.Path);
                
                // Fail securely - return 500 error
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                await context.Response.WriteAsync("Security validation failed");
            }
        }

        /// <summary>
        /// Builds security context from HTTP request.
        /// </summary>
        private async Task<SecurityRequestContext> BuildSecurityContextAsync(HttpContext context)
        {
            var request = context.Request;
            var requestBody = "";

            // Read request body if it's a POST/PUT/PATCH request
            if (HttpMethods.IsPost(request.Method) || 
                HttpMethods.IsPut(request.Method) || 
                HttpMethods.IsPatch(request.Method))
            {
                request.EnableBuffering();
                using var reader = new StreamReader(request.Body, leaveOpen: true);
                requestBody = await reader.ReadToEndAsync();
                request.Body.Position = 0;
            }

            return new SecurityRequestContext
            {
                IpAddress = GetClientIpAddress(context),
                UserAgent = request.Headers.UserAgent.ToString(),
                UserId = context.User?.Identity?.Name ?? "",
                Endpoint = $"{request.Method} {request.Path}",
                HttpMethod = request.Method,
                Headers = request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()),
                RequestBody = requestBody,
                RequestTime = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Handles security violations by blocking requests and logging events.
        /// </summary>
        private async Task HandleSecurityViolationAsync(
            HttpContext context, 
            SecurityValidationResult validationResult,
            ISecurityService securityService)
        {
            var ipAddress = GetClientIpAddress(context);

            // Determine response based on violation type
            if (validationResult.IsBlocked)
            {
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                await context.Response.WriteAsync("Access denied: IP address blocked");
                
                _logger.LogWarning("Blocked request from {IpAddress} on {Path}: IP address is blocked",
                    ipAddress, context.Request.Path);
            }
            else if (validationResult.IsRateLimited)
            {
                context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
                context.Response.Headers.Add("Retry-After", validationResult.RetryAfter?.TotalSeconds.ToString());
                await context.Response.WriteAsync("Rate limit exceeded. Please try again later.");
                
                _logger.LogWarning("Rate limited request from {IpAddress} on {Path}: {Message}",
                    ipAddress, context.Request.Path, validationResult.Message);
            }
            else if (validationResult.Threats.Any(t => t.Severity >= ThreatSeverity.High))
            {
                // Block IP for high severity threats
                await securityService.BlockIpAddressAsync(
                    ipAddress, 
                    $"High severity threats detected: {string.Join(", ", validationResult.Threats.Select(t => t.Type))}", 
                    TimeSpan.FromHours(1));

                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                await context.Response.WriteAsync("Access denied: Security threats detected");
                
                _logger.LogError("Blocked and banned IP {IpAddress} on {Path}: High severity threats detected: {Threats}",
                    ipAddress, context.Request.Path, 
                    string.Join(", ", validationResult.Threats.Select(t => $"{t.Type}({t.Severity})")));
            }
            else
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await context.Response.WriteAsync("Bad request: Security validation failed");
                
                _logger.LogWarning("Rejected request from {IpAddress} on {Path}: {Message}",
                    ipAddress, context.Request.Path, validationResult.Message);
            }

            // Log detailed security event
            await securityService.LogSecurityEventAsync(new SecurityEvent
            {
                EventType = SecurityEventType.ThreatDetected,
                IpAddress = ipAddress,
                UserAgent = context.Request.Headers.UserAgent.ToString(),
                Description = validationResult.Message,
                Details = JsonSerializer.Serialize(new
                {
                    Endpoint = context.Request.Path.ToString(),
                    Method = context.Request.Method,
                    Threats = validationResult.Threats,
                    IsBlocked = validationResult.IsBlocked,
                    IsRateLimited = validationResult.IsRateLimited
                }),
                Severity = validationResult.Threats.Any() 
                    ? validationResult.Threats.Max(t => t.Severity) 
                    : ThreatSeverity.Medium
            });
        }

        /// <summary>
        /// Adds security headers to the response.
        /// </summary>
        private static void AddSecurityHeaders(HttpResponse response)
        {
            // Content Security Policy
            response.Headers["Content-Security-Policy"] = 
                "default-src 'self'; " +
                "script-src 'self' 'unsafe-inline' 'unsafe-eval'; " +
                "style-src 'self' 'unsafe-inline'; " +
                "img-src 'self' data: https:; " +
                "font-src 'self' data:; " +
                "connect-src 'self'; " +
                "frame-ancestors 'none'";

            // Security headers
            response.Headers["X-Content-Type-Options"] = "nosniff";
            response.Headers["X-Frame-Options"] = "DENY";
            response.Headers["X-XSS-Protection"] = "1; mode=block";
            response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
            response.Headers["Permissions-Policy"] = "geolocation=(), microphone=(), camera=()";
            
            // Remove server information
            response.Headers.Remove("Server");
            response.Headers.Remove("X-Powered-By");
            
            // HSTS (HTTP Strict Transport Security) - only for HTTPS
            if (response.HttpContext.Request.IsHttps)
            {
                response.Headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";
            }
        }

        /// <summary>
        /// Gets the real client IP address, handling proxies and load balancers.
        /// </summary>
        private static string GetClientIpAddress(HttpContext context)
        {
            // Check for X-Forwarded-For header (load balancers, proxies)
            var xForwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(xForwardedFor))
            {
                // Take the first IP (original client)
                return xForwardedFor.Split(',')[0].Trim();
            }

            // Check for X-Real-IP header (nginx proxy)
            var xRealIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
            if (!string.IsNullOrEmpty(xRealIp))
            {
                return xRealIp;
            }

            // Check for CF-Connecting-IP header (Cloudflare)
            var cfConnectingIp = context.Request.Headers["CF-Connecting-IP"].FirstOrDefault();
            if (!string.IsNullOrEmpty(cfConnectingIp))
            {
                return cfConnectingIp;
            }

            // Fallback to remote IP address
            return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }

        /// <summary>
        /// Determines if security checks should be skipped for certain paths.
        /// </summary>
        private static bool ShouldSkipSecurityCheck(PathString path)
        {
            var skipPaths = new[]
            {
                "/health",
                "/healthcheck",
                "/_blazor",
                "/css",
                "/js",
                "/images",
                "/favicon.ico",
                "/robots.txt",
                "/sitemap.xml"
            };

            return skipPaths.Any(skipPath => path.StartsWithSegments(skipPath, StringComparison.OrdinalIgnoreCase));
        }
    }

    /// <summary>
    /// Extension methods for registering security middleware.
    /// </summary>
    public static class SecurityMiddlewareExtensions
    {
        /// <summary>
        /// Registers the security middleware in the application pipeline.
        /// </summary>
        public static IApplicationBuilder UseSecurityMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<SecurityMiddleware>();
        }
    }
}