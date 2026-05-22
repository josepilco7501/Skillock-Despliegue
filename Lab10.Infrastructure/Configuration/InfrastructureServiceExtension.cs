using Lab10.Infrastructure.Context;

namespace Lab10.Infrastructure.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
// Asegúrate de cambiar estos namespaces por los nombres reales de tus carpetas/proyectos
// using TrainingCenter.Infrastructure.Context; 
// using TrainingCenter.Domain.Interfaces;
// using TrainingCenter.Infrastructure.Repositories;


public static class InfrastructureServicesExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Dabase connection
        services.AddDbContext<Lab10Context>((serviceProvider, options) =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            options.UseSqlServer(connectionString);
        });

        // Services register
        // services.AddTransient<IUnitOfWork, UnitOfWork>();
        // services.AddScoped<IAuthService, AuthService>();
        // services.AddScoped<IFileService, FileService>();
        // services.AddScoped<IUploadFileToAzureStorageService, UploadFileToAzureStorageService>();
        // services.AddScoped<IActivityService, ActivityService>();

        return services;
    }
}

