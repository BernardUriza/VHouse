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

// Register services with their interfaces
builder.Services.AddScoped<IChatbotService, ChatbotService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();

// Register repositories and unit of work
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Add FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<ProductValidator>();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();

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
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(databaseUrl));

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

// Add security headers middleware
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
    context.Response.Headers.Add("Permissions-Policy", "camera=(), microphone=(), geolocation=()");
    
    if (!app.Environment.IsDevelopment())
    {
        context.Response.Headers.Add("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
    }
    
    await next();
});

// 🔧 Aplica migraciones automáticamente en cada inicio
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();

    using var scope = app.Services.CreateScope();
    var migrationLogger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    migrationLogger.LogInformation("📦 Applying migrations...");
    context.Database.Migrate(); // Aplica las migraciones
    migrationLogger.LogInformation("✅ Migrations applied successfully.");
}
using (var scope = app.Services.CreateScope())
{
    var productService = scope.ServiceProvider.GetRequiredService<ProductService>();
    var scopeFactory = scope.ServiceProvider.GetRequiredService<IServiceScopeFactory>();

    migrationLogger.LogInformation("📦 Applying seeds...");
    await productService.SeedProductsAsync(scopeFactory); // ✅ Use Scoped DbContext
    migrationLogger.LogInformation("✅ Seeds applied successfully.");
}

app.UseStaticFiles();
app.UseRouting();

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

// 🔧 Initialize roles and admin user
using (var scope = app.Services.CreateScope())
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
}

app.Run();
