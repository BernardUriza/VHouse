using Microsoft.EntityFrameworkCore;
using VHouse.Components;

var builder = WebApplication.CreateBuilder(args);// Obtener la conexión desde la variable de entorno en Fly.io
var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL")
    ?? builder.Configuration.GetConnectionString("DefaultConnection");

builder.WebHost.UseWebRoot("wwwroot");
builder.WebHost.UseStaticWebAssets();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddHttpClient();
builder.Services.AddScoped<ChatbotService>(); 
builder.Services.AddSingleton<ProductService>();
builder.Services.AddHttpContextAccessor();

string dbPath = builder.Environment.IsDevelopment()
    ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "sqlite_data", "mydatabase.db")
    : "/data/mydatabase.db";

// Asegurar que la carpeta existe en local
if (builder.Environment.IsDevelopment())
{
    string directory = Path.GetDirectoryName(dbPath);
    if (!Directory.Exists(directory))
    {
        Directory.CreateDirectory(directory);
    }
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite($"Data Source={dbPath}")); // ✅ Usa SQLite correctamente

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
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

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

app.Run();
