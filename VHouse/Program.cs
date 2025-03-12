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

// 📌 Obtiene la conexión desde appsettings.json o Fly.io
string? connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DATABASE_URL")))
{
    connectionString = Environment.GetEnvironmentVariable("DATABASE_URL")
        .Replace("postgres://", "Host=")
        .Replace(":", ";Port=")
        .Replace("@", ";Username=")
        .Replace(";", ";Password=") + ";Database=vhouse_dev;Pooling=true;";
}

// 📌 Configura Entity Framework con PostgreSQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

var app = builder.Build();

// 🔧 Aplica migraciones automáticamente en cada inicio
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();

    context.Database.Migrate(); // Aplica las migraciones
}

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
