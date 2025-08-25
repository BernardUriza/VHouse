using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using VHouse;
using VHouse.Classes;
using VHouse.Components;
using VHouse.Services;
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
builder.Services.AddScoped<ChatbotService>();
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<CustomerService>();
builder.Services.AddHttpContextAccessor();

Console.WriteLine("🚀 Iniciando VHouse...");

// 📌 Obtener la cadena de conexión
string? databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
if (!string.IsNullOrEmpty(databaseUrl))
{
    Console.WriteLine($"🌍 DATABASE_URL encontrada: {databaseUrl}");

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
        Console.WriteLine("✅ Connection String generada correctamente.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ ERROR: No se pudo procesar DATABASE_URL: {ex.Message}");
    }
}
else
{
    Console.WriteLine("⚠️ No se encontró DATABASE_URL. Usando configuración por defecto.");
    databaseUrl = builder.Configuration.GetConnectionString("DefaultConnection");
    
    // Replace environment variable placeholder with actual value
    var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD");
    if (!string.IsNullOrEmpty(dbPassword))
    {
        databaseUrl = databaseUrl?.Replace("${DB_PASSWORD}", dbPassword);
    }
    else
    {
        Console.WriteLine("⚠️ DB_PASSWORD environment variable not set. Using default password.");
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
        Console.WriteLine($"🔄 Intentando conectar a PostgreSQL... (Intento {attempt + 1}/{maxRetries})");
        using var testConnection = new NpgsqlConnection(databaseUrl);
        testConnection.Open();
        Console.WriteLine("✅ Conexión exitosa a PostgreSQL.");
        connected = true;
        break;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"⚠️ No se pudo conectar a PostgreSQL: {ex.Message}");
        attempt++;
        Thread.Sleep(3000); // Esperar 3 segundos antes de reintentar
    }
}

if (!connected)
{
    Console.WriteLine("❌ ERROR: No se pudo conectar a PostgreSQL después de varios intentos. Abortando.");
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

    Console.WriteLine("📦 Aplicando migraciones...");
    context.Database.Migrate(); // Aplica las migraciones
    Console.WriteLine("✅ Migraciones aplicadas correctamente.");
}
using (var scope = app.Services.CreateScope())
{
    var productService = scope.ServiceProvider.GetRequiredService<ProductService>();
    var scopeFactory = scope.ServiceProvider.GetRequiredService<IServiceScopeFactory>();

    Console.WriteLine("📦 Aplying semillas...");
    await productService.SeedProductsAsync(scopeFactory); // ✅ Use Scoped DbContext
    Console.WriteLine("✅ Semillas aplicadas correctamente.");
}

app.UseStaticFiles();
app.UseRouting();

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
    
    Console.WriteLine("🔐 Inicializando roles y usuario administrador...");
    
    // Create roles if they don't exist
    string[] roles = { "Admin", "Employee", "Customer" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
            Console.WriteLine($"✅ Rol creado: {role}");
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
            Console.WriteLine($"✅ Usuario administrador creado: {adminEmail}");
            Console.WriteLine("🔑 Password: Admin123!");
        }
        else
        {
            Console.WriteLine("❌ Error creando usuario administrador:");
            foreach (var error in result.Errors)
            {
                Console.WriteLine($"   - {error.Description}");
            }
        }
    }
    else
    {
        Console.WriteLine("ℹ️ Usuario administrador ya existe.");
    }
}

app.Run();
