using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VHouse.Domain.Interfaces;
using VHouse.Infrastructure.Data;
using VHouse.Infrastructure.Repositories;
using VHouse.Infrastructure.Services;

namespace VHouse.Infrastructure;

public static class InfrastructureServiceRegistration
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Database
        services.AddDbContext<VHouseDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("DefaultConnection")));

        // Repositories
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // AI Services - Claude priority with OpenAI fallback
        services.AddHttpClient<IAIService, AIService>(client =>
        {
            client.Timeout = TimeSpan.FromMilliseconds(
                configuration.GetValue<int>("AI:RequestTimeout", 30000));
        });
        services.AddScoped<IAIService, AIService>();

        return services;
    }
}