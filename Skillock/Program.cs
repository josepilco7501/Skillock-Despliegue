using Hangfire;
using Skillock_ProyectoFinal.Configuration;
using Microsoft.EntityFrameworkCore;
using Skillock.Infrastructure.Context;

var builder = WebApplication.CreateBuilder(args);

// 1. Centraliza la inyección de dependencias (Aquí adentro ya se añade Swagger y Controllers)
builder.Services.AddApplicationServices(builder.Configuration);

// 2. Agregar el servicio de CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirFrontendPython", policy =>
    {
        policy.WithOrigins(
                "http://localhost:8000",
                "http://127.0.0.1:8000",
                "http://0.0.0",
                "https://skillock-despliegue.onrender.com",
                "https://skillock-frontend-despliegue.onrender.com")
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// ¡SE ELIMINARON LAS LÍNEAS DUPLICADAS DE ADDCONTROLLERS Y ADDSWAGGERGEN DE AQUÍ!

var app = builder.Build();

// Aplicar migraciones automáticamente
try
{
    var applyMigrations = Environment.GetEnvironmentVariable("APPLY_MIGRATIONS");
    if (!string.IsNullOrEmpty(applyMigrations) && applyMigrations == "true")
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SkillockDbContext>();
        db.Database.Migrate();
    }
}
catch { }

// --- CONFIGURACIÓN CORRECTA DEL PIPELINE VISUAL ---
// Reemplaza tus líneas de UseSwagger por estas tres:
app.UseSwagger(c =>
{
    c.RouteTemplate = "swagger/{documentName}/swagger.json";
});

app.UseSwaggerUI(c =>
{
    // Forzar el mapeo directo para que el catch interno de .NET capture el fallo
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Skillock API v1");
    
    // Si hay un error de mapeo, esto romperá el arranque e imprimirá el log en Rider
    c.ConfigObject.AdditionalItems["throwOnError"] = true; 
});



// Activar CORS
app.UseCors("PermitirFrontendPython");

// Autenticación y Autorización
app.UseAuthentication();  
app.UseAuthorization();   

// Dashboard de Hangfire
app.UseHangfireDashboard(pathMatch: "/hangfire");
app.MapControllers();

app.Run();