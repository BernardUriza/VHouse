using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VHouse.Infrastructure.Data;

namespace VHouse.Tests;

/// <summary>
/// Custom Web Application Factory for testing
/// This factory configures the application with a test database to avoid conflicts
/// with running instances and ensures proper isolation
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the existing database context registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<VHouseDbContext>));

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Add in-memory database for testing
            services.AddDbContext<VHouseDbContext>(options =>
            {
                options.UseInMemoryDatabase("VHouseTestDb");
                options.EnableSensitiveDataLogging();
            });

            // Build service provider and create database
            var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<VHouseDbContext>();
            
            try
            {
                context.Database.EnsureCreated();
            }
            catch (Exception ex)
            {
                // This test will help us identify if there are EF configuration issues
                throw new InvalidOperationException($"Database creation failed: {ex.Message}", ex);
            }
        });

        // Configure test environment
        builder.UseEnvironment("Testing");
        
        // Configure testing-specific settings to skip migrations
        builder.UseSetting("SkipMigrations", "true");
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            try
            {
                // Clean up test database
                using var scope = Services.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<VHouseDbContext>();
                context.Database.EnsureDeleted();
            }
            catch (ObjectDisposedException)
            {
                // Ignore cleanup errors on disposal
            }
        }

        base.Dispose(disposing);
    }
}