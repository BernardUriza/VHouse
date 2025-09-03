// Creado por Bernard Orozco
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using VHouse.Infrastructure.Data;

namespace VHouse.Web.Extensions;

public static class DatabaseConfigurationExtensions
{
    public static WebApplicationBuilder ConfigureDatabase(this WebApplicationBuilder builder)
    {
        var logger = LoggerFactory.Create(b => b.AddConsole()).CreateLogger("VHouse.Database");
        
        var databaseUrl = BuildConnectionString(builder.Configuration, logger);
        
        if (builder.Environment.IsDevelopment())
        {
            builder.Services.AddDbContext<VHouseDbContext>(options =>
            {
                options.UseSqlite(databaseUrl);
                options.EnableSensitiveDataLogging(true);
                options.EnableDetailedErrors(true);
            });
        }
        else
        {
            builder.Services.AddDbContext<VHouseDbContext>(options =>
            {
                options.UseSqlite(databaseUrl);
                options.EnableServiceProviderCaching();
                options.EnableSensitiveDataLogging(false);
            });
        }

        return builder;
    }

    private static string BuildConnectionString(IConfiguration configuration, ILogger logger)
    {
        string? databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
        
        if (!string.IsNullOrEmpty(databaseUrl))
        {
            Log.DatabaseUrlFound(logger, databaseUrl);
            return ProcessDatabaseUrl(databaseUrl, logger);
        }

        Log.DatabaseUrlNotFound(logger);
        databaseUrl = configuration.GetConnectionString("DefaultConnection");

        var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD");
        if (!string.IsNullOrEmpty(dbPassword))
        {
            databaseUrl = databaseUrl?.Replace("${DB_PASSWORD}", dbPassword);
        }
        else
        {
            Log.DbPasswordNotSet(logger);
            throw new InvalidOperationException("DB_PASSWORD environment variable is required for production");
        }

        Log.UsingSqliteDatabase(logger, databaseUrl ?? string.Empty);
        return databaseUrl ?? string.Empty;
    }

    private static string ProcessDatabaseUrl(string databaseUrl, ILogger logger)
    {
        try
        {
            var uri = new Uri(databaseUrl);
            var userInfo = uri.UserInfo.Split(':');

            string host = uri.Host;
            string port = uri.Port.ToString(CultureInfo.InvariantCulture);
            string username = userInfo[0];
            string password = userInfo[1];
            string database = "vhouse-dev-new";
            
            var processedUrl = $"Host={host};Port={port};Username={username};Password={password};Database={database};Pooling=true;Ssl Mode=Disable;Trust Server Certificate=true;";
            Log.ConnectionStringGenerated(logger);
            return processedUrl;
        }
        catch (Exception ex)
        {
            Log.CouldNotProcessDatabaseUrl(logger, ex);
            return databaseUrl;
        }
    }
}

static partial class Log
{
    [LoggerMessage(2, LogLevel.Information, "🌍 DATABASE_URL found: {DatabaseUrl}")]
    public static partial void DatabaseUrlFound(ILogger logger, string databaseUrl);
    
    [LoggerMessage(3, LogLevel.Information, "✅ Connection string generated successfully.")]
    public static partial void ConnectionStringGenerated(ILogger logger);
    
    [LoggerMessage(4, LogLevel.Error, "❌ Could not process DATABASE_URL")]
    public static partial void CouldNotProcessDatabaseUrl(ILogger logger, Exception ex);
    
    [LoggerMessage(5, LogLevel.Information, "⚠️ DATABASE_URL not found. Using default configuration.")]
    public static partial void DatabaseUrlNotFound(ILogger logger);
    
    [LoggerMessage(6, LogLevel.Warning, "⚠️ DB_PASSWORD environment variable not set. Using default password.")]
    public static partial void DbPasswordNotSet(ILogger logger);
    
    [LoggerMessage(7, LogLevel.Information, "✅ Using SQLite database: {DatabaseUrl}")]
    public static partial void UsingSqliteDatabase(ILogger logger, string databaseUrl);
}