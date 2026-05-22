using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Lab10.Infrastructure.Configuration;


namespace Lab10.Configuration;

    public static class ServiceRegistrationExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            // 1. Habilitar HttpContextContextor
            services.AddHttpContextAccessor();
            
            // Registra el servicio ClientContextProvider (si lo usas en tu lab para capturar headers)
            // services.AddScoped<IClientContextProvider, ClientContextProvider>();

            // 2. Registro de servicios de la capa de Infraestructura (llamada al archivo anterior)
            services.AddInfrastructureServices(configuration);

            // 3. Configuración de autenticación con JWT
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = "LanguageBridgeApplications.com", // Cambiar por tu Issuer
                        ValidAudience = "LanguageBridgeApplications.com", // Cambiar por tu Audience
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:SecretKey"]))
                    };
                });

            // 4. Habilitar controladores del API
            services.AddControllers();

            // 5. Configuración y habilitación de Swagger
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(options =>
            {
                // Permite personalizar las opciones de Swagger
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "My API",
                    Version = "v1",
                    Description = "API para gestionar recursos."
                });
            });

            return services;
        }
    }

