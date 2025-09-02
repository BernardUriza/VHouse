using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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
        // Load .env file for testing
        LoadEnvFile();
        
        // Ensure environment variables are available to IConfiguration
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddEnvironmentVariables();
        });
        
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

    private static void LoadEnvFile()
    {
        var currentDir = Directory.GetCurrentDirectory();
        
        // Try different possible paths for .env file
        var possiblePaths = new[]
        {
            Path.Combine(currentDir, ".env"), // Same directory as test
            Path.Combine(currentDir, "..", ".env"), // One level up
            Path.Combine(currentDir, "..", "..", ".env"), // Two levels up
            Path.Combine(currentDir, "..", "..", "..", "..", ".env"), // Four levels up (original)
            Path.Combine(currentDir, "..", "..", "..", "..", "..", ".env") // Five levels up
        };
        
        Console.WriteLine($"🔍 Test Current directory: {currentDir}");
        
        string envFile = null;
        foreach (var path in possiblePaths)
        {
            Console.WriteLine($"🔍 Test Trying path: {path}");
            Console.WriteLine($"🔍 Test Path exists: {File.Exists(path)}");
            if (File.Exists(path))
            {
                envFile = path;
                Console.WriteLine($"✅ Test Found .env at: {envFile}");
                break;
            }
        }

        if (!string.IsNullOrEmpty(envFile) && File.Exists(envFile))
        {
            Console.WriteLine($"📁 Test Loading .env file...");
            foreach (var line in File.ReadAllLines(envFile))
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#')) continue;
                
                var parts = line.Split('=', 2);
                if (parts.Length == 2)
                {
                    var key = parts[0].Trim();
                    var value = parts[1].Trim();
                    Environment.SetEnvironmentVariable(key, value);
                    if (key.Contains("CLAUDE"))
                    {
                        Console.WriteLine($"🔑 Test Set {key}: {value.Substring(0, Math.Min(10, value.Length))}...");
                    }
                }
            }
        }
        else
        {
            Console.WriteLine($"❌ Test .env file not found in any of the searched paths!");
        }
    }
}