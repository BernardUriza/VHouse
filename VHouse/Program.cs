using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using VHouse;
using VHouse.Classes;
using VHouse.Components;
using VHouse.Interfaces;
using VHouse.Middleware;
using VHouse.Repositories;
using VHouse.Services;
using VHouse.Validators;
using Npgsql;
using System.Diagnostics;

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

// Register services with their interfaces
builder.Services.AddScoped<IChatbotService, ChatbotService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();

// Register B2B services
builder.Services.AddScoped<ISupplierService, SupplierService>();
builder.Services.AddScoped<IBrandService, BrandService>();
builder.Services.AddScoped<IPurchaseOrderService, PurchaseOrderService>();
builder.Services.AddScoped<IWarehouseService, WarehouseService>();
builder.Services.AddScoped<IShrinkageService, ShrinkageService>();

// Phase 5: Advanced Distribution Services
builder.Services.AddScoped<ITenantService, TenantService>();
builder.Services.AddScoped<IDistributionCenterService, DistributionCenterService>();
builder.Services.AddScoped<IRouteOptimizationService, RouteOptimizationService>();
builder.Services.AddScoped<IInventorySynchronizationService, InventorySynchronizationService>();

// Phase 6: Production Security Framework
builder.Services.AddScoped<ISecurityService, SecurityService>();
builder.Services.AddScoped<IMonitoringService, MonitoringService>();
builder.Services.AddScoped<IBackupService, BackupService>();

// Phase 7: Advanced Analytics & Business Intelligence
// Note: Temporarily disabled due to model conflicts
// builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();
// builder.Services.AddScoped<IBusinessIntelligenceService, BusinessIntelligenceService>();
// builder.Services.AddScoped<IPredictionService, PredictionService>();
// builder.Services.AddScoped<IDataWarehouseService, DataWarehouseService>();

// Phase 8: Multi-Cloud & Hybrid Infrastructure
// Note: Temporarily disabled due to model conflicts
// builder.Services.AddScoped<ICloudOrchestrationService, CloudOrchestrationService>();
// builder.Services.AddScoped<IInfrastructureService, InfrastructureService>();
// builder.Services.AddScoped<IContainerOrchestrationService, ContainerOrchestrationService>();

// Phase 9: Advanced AI & Machine Learning Platform
// Note: AI services temporarily disabled for demo due to model conflicts
// builder.Services.AddScoped<IAIOrchestrationService, AIOrchestrationService>();
// builder.Services.AddScoped<INLPService, NLPService>();
// builder.Services.AddScoped<IRecommendationService, RecommendationService>();
// builder.Services.AddScoped<IComputerVisionService, ComputerVisionService>();

// Phase 10: Enterprise Ecosystem & API Economy
builder.Services.AddScoped<IAPIGatewayService, APIGatewayService>();
builder.Services.AddScoped<IIntegrationService, IntegrationService>();
builder.Services.AddScoped<IBlockchainService, BlockchainService>();

// Register caching services (safe for dev)
builder.Services.AddScoped<ICachingService, CachingService>();

// Register background job service
if (!builder.Environment.IsDevelopment())
{
    builder.Services.AddSingleton<IBackgroundJobService, BackgroundJobService>();
    builder.Services.AddHostedService<BackgroundJobService>();
}

// Add compression (safe for dev)
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.BrotliCompressionProvider>();
    options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProvider>();
});

// Register repositories and unit of work
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Add FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<ProductValidator>();

// Add localization
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[] { "en-US", "es-MX" };
    options.SetDefaultCulture(supportedCultures[0])
        .AddSupportedCultures(supportedCultures)
        .AddSupportedUICultures(supportedCultures);
});

builder.Services.AddHttpContextAccessor();

var logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger("VHouse.Startup");
logger.LogInformation("🚀 Starting VHouse...");

// 📌 Obtener la cadena de conexión
string? databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
if (!string.IsNullOrEmpty(databaseUrl))
{
    logger.LogInformation("🌍 DATABASE_URL found: {DatabaseUrl}", databaseUrl);

    try
    {
        var uri = new Uri(databaseUrl);
        var userInfo = uri.UserInfo.Split(':');

        string host = uri.Host;
        string port = uri.Port.ToString();
        string username = userInfo[0];
        string password = userInfo[1];
        string database = "vhouse-dev-new"; // Asegúrate de usar el nombre correcto
        databaseUrl = $"Host={host};Port={port};Username={username};Password={password};Database={database};Pooling=true;Ssl Mode=Disable;Trust Server Certificate=true;";
        logger.LogInformation("✅ Connection string generated successfully.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "❌ Could not process DATABASE_URL");
    }
}
else
{
    logger.LogInformation("⚠️ DATABASE_URL not found. Using default configuration.");
    databaseUrl = builder.Configuration.GetConnectionString("DefaultConnection");
    
    // Replace environment variable placeholder with actual value
    var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD");
    if (!string.IsNullOrEmpty(dbPassword))
    {
        databaseUrl = databaseUrl?.Replace("${DB_PASSWORD}", dbPassword);
    }
    else
    {
        logger.LogWarning("⚠️ DB_PASSWORD environment variable not set. Using default password.");
        databaseUrl = databaseUrl?.Replace("${DB_PASSWORD}", "mysecretpassword");
    }
}

// 🚀 Iniciar PostgreSQL automáticamente en desarrollo
if (builder.Environment.IsDevelopment())
{
    try
    {
        logger.LogInformation("🔍 Starting PostgreSQL for development...");
        var startInfo = new ProcessStartInfo
        {
            FileName = "bash",
            Arguments = "../start-postgres.sh",
            WorkingDirectory = builder.Environment.ContentRootPath,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        using var process = Process.Start(startInfo);
        if (process != null)
        {
            await process.WaitForExitAsync();
            if (process.ExitCode == 0)
            {
                logger.LogInformation("✅ PostgreSQL startup script completed successfully.");
            }
            else
            {
                logger.LogWarning("⚠️ PostgreSQL startup script completed with warnings.");
            }
        }
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "⚠️ Could not run PostgreSQL startup script. Continuing anyway...");
    }
}

// 🛠 Reintentar conexión a la base de datos antes de rendirse
const int maxRetries = 5;
int attempt = 0;
bool connected = false;

while (attempt < maxRetries)
{
    try
    {
        logger.LogInformation("🔄 Attempting to connect to PostgreSQL... (Attempt {Attempt}/{MaxRetries})", attempt + 1, maxRetries);
        using var testConnection = new NpgsqlConnection(databaseUrl);
        testConnection.Open();
        logger.LogInformation("✅ Successfully connected to PostgreSQL.");
        connected = true;
        break;
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "⚠️ Could not connect to PostgreSQL");
        attempt++;
        Thread.Sleep(3000); // Esperar 3 segundos antes de reintentar
    }
}

if (!connected)
{
    logger.LogError("❌ Could not connect to PostgreSQL after several attempts. Aborting.");
    return;
}

// 📌 Configurar Entity Framework con PostgreSQL
if (builder.Environment.IsDevelopment())
{
    // Configuración para desarrollo con logging básico
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
    {
        options.UseNpgsql(databaseUrl);
        options.EnableSensitiveDataLogging(true);
        options.EnableDetailedErrors(true);
    });
}
else
{
    // Configuración avanzada para producción
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
    {
        options.UseNpgsql(databaseUrl, npgsqlOptions =>
        {
            npgsqlOptions.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorCodesToAdd: null);
            npgsqlOptions.CommandTimeout(30);
        });
        
        options.EnableServiceProviderCaching();
        options.EnableSensitiveDataLogging(false);
    });
}

// Add health checks after databaseUrl is configured
if (!builder.Environment.IsDevelopment())
{
    builder.Services.AddHealthChecks()
        .AddNpgSql(databaseUrl ?? "Host=localhost;Database=vhouse;Username=postgres;Password=postgres", name: "postgresql", tags: new[] { "database", "postgresql" })
        .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy(), tags: new[] { "self" });
}

// 🔐 Configure Identity services
builder.Services.AddDefaultIdentity<ApplicationUser>(options => {
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
.AddEntityFrameworkStores<ApplicationDbContext>();

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
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
app.UseSecurityMiddleware();

// Add security headers middleware
app.Use(async (context, next) =>
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
    
    await next();
});

// 🔧 Aplica migraciones automáticamente en cada inicio
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    var migrationLogger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    migrationLogger.LogInformation("📦 Applying migrations...");
    context.Database.Migrate(); // Aplica las migraciones
    migrationLogger.LogInformation("✅ Migrations applied successfully.");

    var productService = scope.ServiceProvider.GetRequiredService<IProductService>();
    var scopeFactory = scope.ServiceProvider.GetRequiredService<IServiceScopeFactory>();

    migrationLogger.LogInformation("📦 Applying seeds...");
    await productService.SeedProductsAsync(scopeFactory); // ✅ Use Scoped DbContext
    migrationLogger.LogInformation("✅ Seeds applied successfully.");
}

// Add compression middleware (safe for dev)
app.UseResponseCompression();

app.UseStaticFiles();

// Add response caching middleware (safe for dev)
app.UseResponseCaching();

app.UseRouting();

// Add health check endpoint (solo en producción)
if (!app.Environment.IsDevelopment())
{
    app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
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

app.MapRazorComponents<App>()
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
