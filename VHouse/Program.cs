using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using VHouse;
using VHouse.Components;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseWebRoot("wwwroot");
builder.WebHost.UseStaticWebAssets();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddHttpClient();
builder.Services.AddScoped<ChatbotService>();
builder.Services.AddScoped<ProductService>();
builder.Services.AddHttpContextAccessor();

string dbPath = builder.Environment.IsDevelopment()
    ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "sqlite_data", "mydatabase.db")
    : "/data/mydatabase.db";

if (builder.Environment.IsDevelopment())
{
    string directory = Path.GetDirectoryName(dbPath);
    if (!Directory.Exists(directory))
    {
        Directory.CreateDirectory(directory);
    }
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite($"Data Source={dbPath}")); 

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();

    if (!File.Exists(dbPath))
    {
        context.Database.Migrate();

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
        else
        {
            app.Logger.LogInformation("✅ La base de datos ya tiene productos.");
        }
    }
    else
    {
        app.Logger.LogInformation("📂 Base de datos encontrada, no se ejecuta `Migrate()`.");
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
