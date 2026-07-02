using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Skillock.Application.Interfaces;
using Skillock.Application.Interfaces.Services;
using Skillock.Domain.Interfaces;
using Skillock.Infrastructure.Context;
using Skillock.Infrastructure.Jobs;
using Skillock.Infrastructure.Persistence;
using Skillock.Infrastructure.Persistence.Repositories;
using Skillock.Infrastructure.Services;
using Skillock.Infrastructure.Services.EsportApiClients;

namespace Skillock.Infrastructure.Configuration;

/// <summary>
/// Extension method para registrar servicios de Infrastructure en el contenedor de DI.
/// </summary>
public static class InfrastructureServiceExtension
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services, IConfiguration configuration)
    {
        // DbContext con retry policy (mejor versión)
        services.AddDbContext<SkillockDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        // Repositorios
        services.AddScoped<IBetRepository, BetRepository>();
        services.AddScoped<IWalletRepository, WalletRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Servicios de aplicación
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IMatchMonitoringService, MatchMonitoringService>();
        services.AddScoped<IBetExpirationService, BetExpirationService>();
        services.AddScoped<IReportsService, ReportsService>();

        // Clientes HTTP de Esports
        services.AddHttpClient<Dota2ApiClient>()
            .ConfigureHttpClient(c => c.Timeout = TimeSpan.FromSeconds(30));
        services.AddHttpClient<CS2ApiClient>()
            .ConfigureHttpClient(c => c.Timeout = TimeSpan.FromSeconds(30));
        services.AddHttpClient<ValorantApiClient>()
            .ConfigureHttpClient(c => c.Timeout = TimeSpan.FromSeconds(30));

        services.AddScoped<IEsportApiClient>(sp => sp.GetRequiredService<Dota2ApiClient>());
        services.AddScoped<IEsportApiClient>(sp => sp.GetRequiredService<CS2ApiClient>());
        services.AddScoped<IEsportApiClient>(sp => sp.GetRequiredService<ValorantApiClient>());

        services.AddScoped<IEsportApiClientFactory, EsportApiClientFactory>();  // ← Scoped, no Singleton
        
        services.AddHangfire(config => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UsePostgreSqlStorage(o =>
                o.UseNpgsqlConnection(configuration.GetConnectionString("DefaultConnection"))));

        services.AddHangfireServer(options =>
        {
            options.WorkerCount = 5;
            options.Queues = new[] { "critical", "default" };
        });

        services.AddScoped<IBetExpirationJob, BetExpirationJob>();
        services.AddScoped<IMatchMonitoringJob, MatchMonitoringJob>();
        services.AddScoped<IMatchTimeoutJob, MatchTimeoutJob>();

        return services;
    }
}
