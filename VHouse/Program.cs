using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using VHouse;
using VHouse.Components;
using VHouse.Services;

var builder = WebApplication.CreateBuilder(args);

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

// 🔄 Detectar si estamos en producción o desarrollo
bool isDevelopment = builder.Environment.IsDevelopment();
string? connectionString;

// 🛢️ PostgreSQL en ambos entornos
if (isDevelopment)
{
    // Configuración para PostgreSQL en desarrollo (local)
    connectionString = builder.Configuration.GetConnectionString("PostgresLocal")
        ?? "Host=localhost;Port=5432;Database=vhouse_dev;Username=postgres;Password=mysecretpassword";
}
else
{
    // Configuración para PostgreSQL en producción (Fly.io)
    connectionString = Environment.GetEnvironmentVariable("DATABASE_URL")?
        .Replace("postgres://", "Host=")
        .Replace(":", ";Port=")
        .Replace("@", ";Username=")
        .Replace(";", ";Password=") + ";Database=postgres;Pooling=true;";

    if (string.IsNullOrEmpty(connectionString))
    {
        throw new InvalidOperationException("❌ No se encontró la variable de entorno DATABASE_URL para PostgreSQL.");
    }

    // 📂 Log en el volumen persistente para verificar reinicios
    string logFile = "/data/deploy_log.txt";
    File.AppendAllText(logFile, $"🚀 Deploy iniciado en UTC: {DateTime.UtcNow}\n");
}

// Configurar Entity Framework con PostgreSQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

var app = builder.Build();

// 📌 Ejecutar migraciones automáticamente en Fly.io y local
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();

    try
    {
        context.Database.Migrate();
        app.Logger.LogInformation("✅ Migraciones aplicadas correctamente.");
    }
    catch (Exception ex)
    {
        app.Logger.LogError("❌ Error al ejecutar migraciones: {Message}", ex.Message);
    }

    // Cargar productos si la base de datos está vacía
    if (!context.Products.Any())
    {
        var env = services.GetRequiredService<IWebHostEnvironment>();
        string jsonPath = Path.Combine(env.WebRootPath, "data", "products.json");

        if (File.Exists(jsonPath))
        {
            var jsonData = File.ReadAllText(jsonPath);
            var products = JsonSerializer.Deserialize<List<Product>>(jsonData);

            if (products != null && products.Count > 0)
            {
                context.Products.AddRange(products);
                context.SaveChanges();
                app.Logger.LogInformation("✅ Productos cargados desde JSON.");
            }
        }
        else
        {
            app.Logger.LogWarning("⚠️ Archivo 'products.json' no encontrado. No se importaron productos.");
        }
    }
}

// 📌 Configuración del pipeline de la aplicación
if (!isDevelopment)
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Logger.LogInformation("🚀 VHouse se está ejecutando en {Environment}...", app.Environment.EnvironmentName);
app.Run();
