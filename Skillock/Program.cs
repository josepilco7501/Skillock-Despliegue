using Hangfire;
using Skillock_ProyectoFinal.Configuration;
using Microsoft.EntityFrameworkCore;
using Skillock.Infrastructure.Context;

var builder = WebApplication.CreateBuilder(args);

// 1. Centraliza la inyección de dependencias usando el método de tu profesor
builder.Services.AddApplicationServices(builder.Configuration);

// 2. Agregar el servicio de CORS
// --- PASO A: AGREGAR EL SERVICIO DE CORS (Debe ir antes de builder.Build()) ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirFrontendPython", policy =>
    {
        policy.WithOrigins(
                "http://localhost:8000",
                "http://127.0.0.1:8000",
                "http://0.0.0",
                "https://skillock-despliegue.onrender.com")
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddControllers();

var app = builder.Build();

// Aplicar migraciones automáticamente si la variable de entorno APPLY_MIGRATIONS está activada.
// Útil para entornos de despliegue donde no se ejecuta `dotnet ef database update` manualmente.
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
catch
{
    // Si falla la migración automática no detenemos el arranque; el error se registrará en logs.
}

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();
// ---ACTIVAR EL MIDDLEWARE DE CORS ---
// ¡MUY IMPORTANTE! Debe ir justo aquí: después de app.Build() y ANTES de app.MapControllers()
app.UseCors("PermitirFrontendPython");

//Para authenticacion
app.UseAuthentication();  
app.UseAuthorization();   

// 3. AGREGAR DASHBOARD DE HANGFIRE AQUÍ (Siempre después de UseAuthorization)
app.UseHangfireDashboard(pathMatch: "/hangfire");
app.MapControllers();

app.Run();