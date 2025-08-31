// Creado por Bernard Orozco
using VHouse.Web.Extensions;
using VHouse.Web.Middleware;
using VHouse.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Load .env file first (if it exists)
var currentDir = Directory.GetCurrentDirectory();
var envFile = Path.Combine(currentDir, "..", ".env");
Console.WriteLine($"🔍 Looking for .env at: {envFile}");
Console.WriteLine($"🔍 Current directory: {currentDir}");
Console.WriteLine($"🔍 .env exists: {File.Exists(envFile)}");

if (File.Exists(envFile))
{
    Console.WriteLine($"📁 Loading .env file...");
    foreach (var line in File.ReadAllLines(envFile))
    {
        if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;
        
        var parts = line.Split('=', 2);
        if (parts.Length == 2)
        {
            var key = parts[0].Trim();
            var value = parts[1].Trim();
            Environment.SetEnvironmentVariable(key, value);
            if (key.Contains("CLAUDE"))
            {
                Console.WriteLine($"🔑 Set {key}: {value.Substring(0, Math.Min(10, value.Length))}...");
            }
        }
    }
}
else
{
    Console.WriteLine($"❌ .env file not found!");
}

builder.Configuration.AddEnvironmentVariables();
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

var logger = LoggerFactory.Create(b => b.AddConsole()).CreateLogger("VHouse.Startup");
Log.StartingVHouse(logger);

builder.ConfigureWebHost()
       .ConfigureDatabase()
       .Services.AddVHouseServices(builder.Configuration, builder.Environment)
       .AddDataSeeder();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
    app.UseHsts();
}

app.UseSecurityHeaders();
app.UseStaticFiles();
app.UseResponseCaching();
app.UseRouting();
app.UseRequestLocalization();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapRazorPages();
app.MapRazorComponents<VHouse.Web.Components.App>()
   .AddInteractiveServerRenderMode();

app.ConfigureHealthChecks();

await app.SeedDataAsync(builder.Configuration);

app.Run();

public partial class Program { }

static partial class Log
{
    [LoggerMessage(1, LogLevel.Information, "🚀 Starting VHouse...")]
    public static partial void StartingVHouse(ILogger logger);
}
