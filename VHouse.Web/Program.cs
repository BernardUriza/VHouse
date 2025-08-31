// Creado por Bernard Orozco
using VHouse.Web.Extensions;
using VHouse.Web.Middleware;
using VHouse.Web.Services;

var builder = WebApplication.CreateBuilder(args);

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
