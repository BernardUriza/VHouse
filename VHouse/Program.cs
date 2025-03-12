using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using VHouse;
using VHouse.Components;
using VHouse.Services;
using Npgsql;

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

var app = builder.Build();

// 🔧 Aplica migraciones automáticamente en cada inicio
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();

    Console.WriteLine("📦 Aplicando migraciones...");
    context.Database.Migrate(); // Aplica las migraciones
    Console.WriteLine("✅ Migraciones aplicadas correctamente.");
}

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
