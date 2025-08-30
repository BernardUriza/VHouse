using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Globalization;
using VHouse.Domain.Interfaces;
using VHouse.Domain.Entities;
using VHouse.Application.Common;
using VHouse.Infrastructure;
using VHouse.Infrastructure.Data;
using VHouse.Application.Services;
// using Npgsql;
using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

// Configure environment variables and user secrets
builder.Configuration.AddEnvironmentVariables();
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

builder.WebHost.UseWebRoot("wwwroot");
builder.WebHost.UseStaticWebAssets();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddHttpClient();

// Add caching
builder.Services.AddMemoryCache();
var redisConnectionString = builder.Configuration.GetConnectionString("Redis");
if (!string.IsNullOrEmpty(redisConnectionString))
{
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redisConnectionString;
        options.InstanceName = "VHouse";
    });
}
else
{
    builder.Services.AddDistributedMemoryCache();
}

// Add response caching
builder.Services.AddResponseCaching();

// Add response compression services
builder.Services.AddResponseCompression();

// Clean Architecture Services
builder.Services.AddScoped<IAIService, VHouse.Infrastructure.Services.AIService>();

// Add MediatR
builder.Services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssembly(typeof(VHouse.Application.Commands.GenerateProductDescriptionCommand).Assembly);
});

// Register Infrastructure Services
builder.Services.AddScoped<IUnitOfWork, VHouse.Infrastructure.Repositories.UnitOfWork>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(VHouse.Infrastructure.Repositories.Repository<>));

// Add Application Services
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

// Add localization
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = Constants.SupportedCultures;
    options.SetDefaultCulture(supportedCultures[0])
        .AddSupportedCultures(supportedCultures)
        .AddSupportedUICultures(supportedCultures);
});

builder.Services.AddHttpContextAccessor();

var logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger("VHouse.Startup");
Log.StartingVHouse(logger);

// 📌 Obtener la cadena de conexión
string? databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
if (!string.IsNullOrEmpty(databaseUrl))
{
    Log.DatabaseUrlFound(logger, databaseUrl);

    try
    {
        var uri = new Uri(databaseUrl);
        var userInfo = uri.UserInfo.Split(':');

        string host = uri.Host;
        string port = uri.Port.ToString(CultureInfo.InvariantCulture);
        string username = userInfo[0];
        string password = userInfo[1];
        string database = "vhouse-dev-new"; // Asegúrate de usar el nombre correcto
        databaseUrl = $"Host={host};Port={port};Username={username};Password={password};Database={database};Pooling=true;Ssl Mode=Disable;Trust Server Certificate=true;";
        Log.ConnectionStringGenerated(logger);
    }
    catch (Exception ex)
    {
        Log.CouldNotProcessDatabaseUrl(logger, ex);
    }
}
else
{
    Log.DatabaseUrlNotFound(logger);
    databaseUrl = builder.Configuration.GetConnectionString("DefaultConnection");
    
    // Replace environment variable placeholder with actual value
    var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD");
    if (!string.IsNullOrEmpty(dbPassword))
    {
        databaseUrl = databaseUrl?.Replace("${DB_PASSWORD}", dbPassword);
    }
    else
    {
        Log.DbPasswordNotSet(logger);
        databaseUrl = databaseUrl?.Replace("${DB_PASSWORD}", "mysecretpassword");
    }
}

// 🚀 SQLite no necesita configuración adicional - se crea automáticamente
Log.UsingSqliteDatabase(logger, databaseUrl ?? string.Empty);

// 📌 Configurar Entity Framework con SQLite
if (builder.Environment.IsDevelopment())
{
    // Configuración para desarrollo con logging básico
    builder.Services.AddDbContext<VHouseDbContext>(options =>
    {
        options.UseSqlite(databaseUrl);
        options.EnableSensitiveDataLogging(true);
        options.EnableDetailedErrors(true);
    });
}
else
{
    // Configuración avanzada para producción
    builder.Services.AddDbContext<VHouseDbContext>(options =>
    {
        options.UseSqlite(databaseUrl);
        
        options.EnableServiceProviderCaching();
        options.EnableSensitiveDataLogging(false);
    });
}

// Add health checks after databaseUrl is configured
builder.Services.AddHealthChecks()
    .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("Application is running"), tags: Constants.SelfHealthTags)
    .AddDbContextCheck<VHouseDbContext>("database", tags: Constants.DatabaseHealthTags);

// 🔐 Configure Identity services
builder.Services.AddDefaultIdentity<IdentityUser>(options => {
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
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.LogoutPath = "/Identity/Account/Logout";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(24);
    options.SlidingExpiration = true;
});

var app = builder.Build();

// 🔒 Configure HTTPS and security headers
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
    app.UseHsts();
}

// Add global exception handling middleware
// app.UseMiddleware<GlobalExceptionHandlingMiddleware>(); // Commented out until middleware is created
// app.UseSecurityMiddleware(); // Commented out until middleware is created

// Add security headers middleware - excluding static files to avoid MIME type issues
app.Use(async (context, next) =>
{
    await next();
    
    // Don't add security headers to static files to avoid MIME type conflicts
    if (!context.Request.Path.StartsWithSegments("/_framework") &&
        !context.Request.Path.StartsWithSegments("/css") &&
        !context.Request.Path.StartsWithSegments("/js") &&
        !context.Request.Path.StartsWithSegments("/lib") &&
        !context.Request.Path.Value.EndsWith(".css") &&
        !context.Request.Path.Value.EndsWith(".js") &&
        !context.Request.Path.Value.EndsWith(".ico") &&
        !context.Request.Path.Value.EndsWith(".png") &&
        !context.Request.Path.Value.EndsWith(".jpg") &&
        !context.Request.Path.Value.EndsWith(".gif"))
    {
        context.Response.Headers["X-Content-Type-Options"] = "nosniff";
        context.Response.Headers["X-Frame-Options"] = "DENY";
        context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
        context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
        context.Response.Headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
        
        if (!app.Environment.IsDevelopment())
        {
            context.Response.Headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";
        }
    }
});

// 🔧 Aplica migraciones automáticamente en cada inicio (skip during testing)
var skipMigrations = builder.Configuration.GetValue<bool>("SkipMigrations");
if (!skipMigrations)
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var context = services.GetRequiredService<VHouseDbContext>();
        var migrationLogger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        
        Log.ApplyingMigrations(migrationLogger);
        context.Database.Migrate(); // Aplica las migraciones
        Log.MigrationsAppliedSuccessfully(migrationLogger);

        // var productService = scope.ServiceProvider.GetRequiredService<IProductService>(); // Commented out until ProductService is available
        var scopeFactory = scope.ServiceProvider.GetRequiredService<IServiceScopeFactory>();

        Log.ApplyingSeeds(migrationLogger);
        await SeedSampleDataAsync(context, migrationLogger);
        Log.SeedsAppliedSuccessfully(migrationLogger);
    }
}

// Add compression middleware (safe for dev)
app.UseResponseCompression();

app.UseStaticFiles();

// Add response caching middleware (safe for dev)
app.UseResponseCaching();

app.UseRouting();

// Add health check endpoints
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains(Constants.SelfHealthTags[0]),
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var response = new { status = report.Status.ToString(), timestamp = DateTime.UtcNow };
        await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response));
    }
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
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
        await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response));
    }
});

// Add detailed health check endpoint (solo en producción)
if (!app.Environment.IsDevelopment())
{
    app.MapHealthChecks("/health", new HealthCheckOptions
    {
        ResponseWriter = async (context, report) =>
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
            await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response));
        }
    });
}

// Add request localization
app.UseRequestLocalization();

// 🔐 Add authentication and authorization middleware
app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

// Map Identity UI pages
app.MapRazorPages();

app.MapRazorComponents<VHouse.Web.Components.App>()
    .AddInteractiveServerRenderMode();

// 🔧 Initialize roles and admin user (disabled temporarily due to connection issues)
// TODO: Re-enable after stabilizing PostgreSQL connection
/*using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    
    var identityLogger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    identityLogger.LogInformation("🔐 Initializing roles and admin user...");
    
    // Create roles if they don't exist
    string[] roles = { "Admin", "Employee", "Customer" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
            identityLogger.LogInformation("✅ Role created: {Role}", role);
        }
    }
    
    // Create admin user if it doesn't exist
    var adminEmail = "admin@vhouse.com";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        adminUser = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true,
            FullName = "Administrador",
            CompanyName = "VHouse"
        };
        
        var result = await userManager.CreateAsync(adminUser, "Admin123!");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
            identityLogger.LogInformation("✅ Admin user created: {AdminEmail}", adminEmail);
            identityLogger.LogInformation("🔑 Password: Admin123!");
        }
        else
        {
            identityLogger.LogError("❌ Error creating admin user:");
            foreach (var error in result.Errors)
            {
                identityLogger.LogError("   - {ErrorDescription}", error.Description);
            }
        }
    }
    else
    {
        identityLogger.LogInformation("ℹ️ Admin user already exists.");
    }
}*/

app.Run();

// Sample data seeding method
static async Task SeedSampleDataAsync(VHouseDbContext context, ILogger logger)
{
    if (!context.Products.Any())
    {
        var sampleProducts = new[]
        {
            new Product
            {
                ProductName = "Queso Vegano Artesanal",
                Emoji = "🧀",
                PriceCost = 80.00m,
                PriceRetail = 120.00m,
                PriceSuggested = 140.00m,
                PricePublic = 150.00m,
                Description = "Delicioso queso vegano hecho con nueces de macadamia",
                StockQuantity = 25,
                IsVegan = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Product
            {
                ProductName = "Hamburguesa Plant-Based",
                Emoji = "🍔",
                PriceCost = 45.00m,
                PriceRetail = 75.00m,
                PriceSuggested = 85.00m,
                PricePublic = 95.00m,
                Description = "Hamburguesa 100% vegetal con proteína de soya",
                StockQuantity = 50,
                IsVegan = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Product
            {
                ProductName = "Leche de Almendra Orgánica",
                Emoji = "🥛",
                PriceCost = 25.00m,
                PriceRetail = 45.00m,
                PriceSuggested = 50.00m,
                PricePublic = 55.00m,
                Description = "Leche de almendra orgánica sin azúcar añadida",
                StockQuantity = 30,
                IsVegan = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Product
            {
                ProductName = "Pizza Vegana Margarita",
                Emoji = "🍕",
                PriceCost = 60.00m,
                PriceRetail = 95.00m,
                PriceSuggested = 110.00m,
                PricePublic = 120.00m,
                Description = "Pizza con queso vegano y albahaca fresca",
                StockQuantity = 15,
                IsVegan = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Product
            {
                ProductName = "Yogurt de Coco Natural",
                Emoji = "🥥",
                PriceCost = 20.00m,
                PriceRetail = 35.00m,
                PriceSuggested = 40.00m,
                PricePublic = 45.00m,
                Description = "Yogurt cremoso de coco natural probiótico",
                StockQuantity = 40,
                IsVegan = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        context.Products.AddRange(sampleProducts);
        await context.SaveChangesAsync();
        logger.LogInformation("✅ Added {ProductCount} sample products to database", sampleProducts.Length);
    }
    else
    {
        logger.LogInformation("ℹ️ Sample products already exist in database");
    }
}

// Make the Program class accessible to the test project
public partial class Program { }

// LoggerMessage delegates for better performance
static partial class Log
{
    [LoggerMessage(1, LogLevel.Information, "🚀 Starting VHouse...")]
    public static partial void StartingVHouse(ILogger logger);
    
    [LoggerMessage(2, LogLevel.Information, "🌍 DATABASE_URL found: {DatabaseUrl}")]
    public static partial void DatabaseUrlFound(ILogger logger, string databaseUrl);
    
    [LoggerMessage(3, LogLevel.Information, "✅ Connection string generated successfully.")]
    public static partial void ConnectionStringGenerated(ILogger logger);
    
    [LoggerMessage(4, LogLevel.Error, "❌ Could not process DATABASE_URL")]
    public static partial void CouldNotProcessDatabaseUrl(ILogger logger, Exception ex);
    
    [LoggerMessage(5, LogLevel.Information, "⚠️ DATABASE_URL not found. Using default configuration.")]
    public static partial void DatabaseUrlNotFound(ILogger logger);
    
    [LoggerMessage(6, LogLevel.Warning, "⚠️ DB_PASSWORD environment variable not set. Using default password.")]
    public static partial void DbPasswordNotSet(ILogger logger);
    
    [LoggerMessage(7, LogLevel.Information, "✅ Using SQLite database: {DatabaseUrl}")]
    public static partial void UsingSqliteDatabase(ILogger logger, string databaseUrl);
    
    [LoggerMessage(8, LogLevel.Information, "📦 Applying migrations...")]
    public static partial void ApplyingMigrations(ILogger logger);
    
    [LoggerMessage(9, LogLevel.Information, "✅ Migrations applied successfully.")]
    public static partial void MigrationsAppliedSuccessfully(ILogger logger);
    
    [LoggerMessage(10, LogLevel.Information, "📦 Applying seeds...")]
    public static partial void ApplyingSeeds(ILogger logger);
    
    [LoggerMessage(11, LogLevel.Information, "✅ Seeds applied successfully.")]
    public static partial void SeedsAppliedSuccessfully(ILogger logger);
}

// Static readonly arrays for better performance  
file static class Constants
{
    public static readonly string[] SupportedCultures = { "en-US", "es-MX" };
    public static readonly string[] SelfHealthTags = { "self" };
    public static readonly string[] DatabaseHealthTags = { "database", "sqlite" };
}
