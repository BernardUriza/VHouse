// Creado por Bernard Orozco
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using System.Text.Json;

namespace VHouse.Web.Extensions;

public static class HealthCheckExtensions
{
    public static WebApplication ConfigureHealthChecks(this WebApplication app)
    {
        app.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("self"),
            ResponseWriter = WriteHealthCheckResponse
        });

        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            ResponseWriter = WriteDetailedHealthCheckResponse
        });

        // Add detailed health check endpoint (production only)
        if (!app.Environment.IsDevelopment())
        {
            app.MapHealthChecks("/health", new HealthCheckOptions
            {
                ResponseWriter = WriteDetailedHealthCheckResponse
            });
        }

        return app;
    }

    private static async Task WriteHealthCheckResponse(HttpContext context, Microsoft.Extensions.Diagnostics.HealthChecks.HealthReport report)
    {
        context.Response.ContentType = "application/json";
        var response = new 
        { 
            status = report.Status.ToString(), 
            timestamp = DateTime.UtcNow 
        };
        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }

    private static async Task WriteDetailedHealthCheckResponse(HttpContext context, Microsoft.Extensions.Diagnostics.HealthChecks.HealthReport report)
    {
        context.Response.ContentType = "application/json";
        var response = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(x => new
            {
                name = x.Key,
                status = x.Value.Status.ToString(),
                exception = x.Value.Exception?.Message,
                duration = x.Value.Duration.ToString()
            }),
            duration = report.TotalDuration.ToString()
        };
        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}