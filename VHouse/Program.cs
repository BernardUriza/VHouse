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

// 📂 Ruta de la base de datos (dentro del volumen en producción)
string dbPath = builder.Environment.IsDevelopment()
    ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "sqlite_data", "mydatabase.db")
    : "/data/mydatabase.db";


// Si estamos en desarrollo, creamos el directorio local
if (builder.Environment.IsDevelopment())
{
    string directory = Path.GetDirectoryName(dbPath);
    if (!Directory.Exists(directory))
    {
        Directory.CreateDirectory(directory);
    }
}
else
{
    // 📂 Archivo de log para verificar si el volumen se mantiene entre deploys
    string logFile = "/data/deploy_log.txt";
    File.AppendAllText(logFile, $"🚀 Deploy iniciado en UTC: {DateTime.UtcNow}\n");
}

    // Configurar Entity Framework con SQLite
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlite($"Data Source={dbPath}"));

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();

    // 💡 Verifica si hay tablas en la BD en lugar de checar si el archivo existe
    var databaseExists = context.Database.GetPendingMigrations().Any() || context.Products.Any();

    if (!databaseExists)
    {
        app.Logger.LogInformation("🆕 Creando nueva base de datos...");
        context.Database.Migrate();

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
    else
    {
        app.Logger.LogInformation("📂 Base de datos ya existente, no se ejecuta `Migrate()`.");
    }
}


// Configuración del pipeline de la aplicación
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStaticFiles();

var httpsPort = Environment.GetEnvironmentVariable("ASPNETCORE_HTTPS_PORT");
if (!string.IsNullOrEmpty(httpsPort))
{
    app.UseHttpsRedirection();
}

app.Logger.LogInformation("🚀 VHouse se está ejecutando en {Environment}...", app.Environment.EnvironmentName);

app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
