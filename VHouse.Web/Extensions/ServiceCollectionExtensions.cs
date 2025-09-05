// Creado por Bernard Orozco
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text.Json;
using VHouse.Domain.Entities;
using VHouse.Domain.Interfaces;
using VHouse.Infrastructure.Data;
using VHouse.Application.Services;
using VHouse.Application.Common;

namespace VHouse.Web.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddVHouseServices(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        services.AddControllers();
        services.AddBlazorServerServices(environment);
        services.AddCachingServices(configuration);
        services.AddApplicationArchitectureServices();
        services.AddLocalizationServices();
        services.AddIdentityServices();
        services.AddHealthCheckServices();
        
        return services;
    }

    private static IServiceCollection AddBlazorServerServices(this IServiceCollection services, IWebHostEnvironment environment)
    {
        services.AddRazorComponents()
            .AddInteractiveServerComponents();

        services.AddSignalR(options =>
        {
            options.EnableDetailedErrors = environment.IsDevelopment();
            options.MaximumReceiveMessageSize = 512 * 1024; // 512KB for larger payloads
            options.StreamBufferCapacity = 20;
            options.ClientTimeoutInterval = TimeSpan.FromSeconds(60);
            options.KeepAliveInterval = TimeSpan.FromSeconds(15);
            options.HandshakeTimeout = TimeSpan.FromSeconds(15);
        }).AddJsonProtocol(options =>
        {
            options.PayloadSerializerOptions.PropertyNamingPolicy = null;
            options.PayloadSerializerOptions.WriteIndented = false;
        });

        services.AddHttpClient();
        
        // Add Markdown service
        services.AddScoped<VHouse.Web.Services.IMarkdownService, VHouse.Web.Services.MarkdownService>();
        
        return services;
    }

    private static IServiceCollection AddCachingServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMemoryCache();
        
        var redisConnectionString = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrEmpty(redisConnectionString))
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnectionString;
                options.InstanceName = "VHouse";
            });
        }
        else
        {
            services.AddDistributedMemoryCache();
        }

        services.AddResponseCaching();
        return services;
    }

    private static IServiceCollection AddApplicationArchitectureServices(this IServiceCollection services)
    {
        // Core MVP Services Only - NO Phase 2 AI features
        services.AddScoped<IUnitOfWork, VHouse.Infrastructure.Repositories.UnitOfWork>();
        services.AddScoped(typeof(IRepository<>), typeof(VHouse.Infrastructure.Repositories.Repository<>));
        services.AddScoped<IPasswordService, VHouse.Infrastructure.Services.PasswordService>();
        
        // Client Inventory System - REAL business logic
        services.AddScoped<VHouse.Application.Services.IClientInventoryService, VHouse.Infrastructure.Services.ClientInventoryService>();
        
        
        // Enterprise Audit and Monitoring - PROFESSIONAL grade
        services.AddScoped<VHouse.Application.Services.IAuditService, VHouse.Infrastructure.Services.AuditService>();
        services.AddScoped<VHouse.Application.Services.IBusinessMetricsService, VHouse.Infrastructure.Services.BusinessMetricsService>();

        // Basic MediatR for CQRS (keeping core architecture)
        services.AddMediatR(cfg => {
            cfg.RegisterServicesFromAssembly(typeof(VHouse.Application.Commands.CreateProductCommand).Assembly);
        });

        return services;
    }

    private static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // This method should be implemented in VHouse.Application project
        // For now, we'll keep it as a placeholder
        return services;
    }

    private static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        // This method should be implemented in VHouse.Infrastructure project
        // For now, we'll keep it as a placeholder
        return services;
    }

    private static IServiceCollection AddLocalizationServices(this IServiceCollection services)
    {
        services.AddLocalization(options => options.ResourcesPath = "Resources");
        services.Configure<RequestLocalizationOptions>(options =>
        {
            var supportedCultures = new[] { "en-US", "es-MX" };
            options.SetDefaultCulture(supportedCultures[0])
                .AddSupportedCultures(supportedCultures)
                .AddSupportedUICultures(supportedCultures);
        });

        services.AddHttpContextAccessor();
        return services;
    }

    private static IServiceCollection AddIdentityServices(this IServiceCollection services)
    {
        services.AddDefaultIdentity<IdentityUser>(options => {
            // Password settings
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 6;
            
            // Lockout settings
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;
            
            // User settings
            options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
            options.User.RequireUniqueEmail = true;
            
            // Email confirmation settings
            options.SignIn.RequireConfirmedEmail = false; // Set to true in production
            options.SignIn.RequireConfirmedAccount = false;
        })
        .AddRoles<IdentityRole>()
        .AddEntityFrameworkStores<VHouseDbContext>();

        // Configure cookie settings
        services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = "/Identity/Account/Login";
            options.LogoutPath = "/Identity/Account/Logout";
            options.AccessDeniedPath = "/Identity/Account/AccessDenied";
            options.ExpireTimeSpan = TimeSpan.FromHours(24);
            options.SlidingExpiration = true;
        });

        return services;
    }

    private static IServiceCollection AddHealthCheckServices(this IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy("Application is running"), tags: new[] { "self" })
            .AddDbContextCheck<VHouseDbContext>("database", tags: new[] { "database", "sqlite" });

        return services;
    }
}