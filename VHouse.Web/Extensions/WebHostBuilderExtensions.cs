// Creado por Bernard Orozco
using Microsoft.AspNetCore.Server.Kestrel.Core;

namespace VHouse.Web.Extensions;

public static class WebHostBuilderExtensions
{
    public static WebApplicationBuilder ConfigureWebHost(this WebApplicationBuilder builder)
    {
        builder.WebHost.UseWebRoot("wwwroot");
        builder.WebHost.UseStaticWebAssets();
        builder.WebHost.ConfigureKestrel(builder.Environment);
        
        return builder;
    }

    private static void ConfigureKestrel(this IWebHostBuilder webHost, IWebHostEnvironment environment)
    {
        webHost.ConfigureKestrel((context, options) =>
        {
            ConfigureKestrelLimits(options);
            ConfigureKestrelPorts(options, environment);
        });
    }

    private static void ConfigureKestrelLimits(KestrelServerOptions options)
    {
        // Increase limits for Blazor Server
        options.Limits.MaxRequestBodySize = 30 * 1024 * 1024; // 30MB
        options.Limits.MaxRequestHeadersTotalSize = 32768;
        options.Limits.MaxRequestLineSize = 8192;
        
        // Keep alive settings for SignalR
        options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(2);
        options.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(1);
    }

    private static void ConfigureKestrelPorts(KestrelServerOptions options, IWebHostEnvironment environment)
    {
        // HTTP port 5000 - HTTP/1.1 only for better Blazor Server compatibility
        options.ListenAnyIP(5000, listenOptions =>
        {
            listenOptions.Protocols = HttpProtocols.Http1;
        });
        
        // HTTPS port 5001 (production only)
        if (!environment.IsDevelopment())
        {
            options.ListenAnyIP(5001, listenOptions =>
            {
                listenOptions.Protocols = HttpProtocols.Http1;
                listenOptions.UseHttps();
            });
        }
    }
}