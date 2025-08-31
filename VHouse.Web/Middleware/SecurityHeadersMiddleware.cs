// Creado por Bernard Orozco
namespace VHouse.Web.Middleware;

public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IWebHostEnvironment _environment;
    
    private static readonly string[] StaticFileExtensions = 
    {
        ".css", ".js", ".ico", ".png", ".jpg", ".gif", ".svg", ".woff", ".woff2", ".ttf", ".eot"
    };
    
    private static readonly string[] ExcludedPaths = 
    {
        "/_framework", "/_blazor", "/css", "/js", "/lib"
    };

    public SecurityHeadersMiddleware(RequestDelegate next, IWebHostEnvironment environment)
    {
        _next = next;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (ShouldAddSecurityHeaders(context.Request))
        {
            AddSecurityHeaders(context.Response);
        }
        
        await _next(context);
    }

    private static bool ShouldAddSecurityHeaders(HttpRequest request)
    {
        var path = request.Path.Value?.ToLowerInvariant() ?? string.Empty;
        
        // Skip security headers for static files and Blazor framework files
        if (ExcludedPaths.Any(excludedPath => path.StartsWith(excludedPath)))
        {
            return false;
        }
        
        if (StaticFileExtensions.Any(ext => path.EndsWith(ext)))
        {
            return false;
        }
        
        return true;
    }

    private void AddSecurityHeaders(HttpResponse response)
    {
        response.Headers["X-Content-Type-Options"] = "nosniff";
        response.Headers["X-Frame-Options"] = "DENY";
        response.Headers["X-XSS-Protection"] = "1; mode=block";
        response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
        response.Headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
        
        if (!_environment.IsDevelopment())
        {
            response.Headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";
        }
    }
}

public static class SecurityHeadersMiddlewareExtensions
{
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<SecurityHeadersMiddleware>();
    }
}