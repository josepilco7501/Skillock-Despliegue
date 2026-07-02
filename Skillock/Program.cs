using Hangfire;
using Skillock_ProyectoFinal.Configuration;

var builder = WebApplication.CreateBuilder(args);

// 1. Centraliza la inyección de dependencias usando el método de tu profesor
builder.Services.AddApplicationServices(builder.Configuration);

// 2. Agregar el servicio de CORS
// --- PASO A: AGREGAR EL SERVICIO DE CORS (Debe ir antes de builder.Build()) ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirFrontendPython", policy =>
    {
        policy.WithOrigins("http://localhost:8000", "http://127.0.0.1:8000","http://0.0.0") // El puerto de tu servidor Python
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
// ---ACTIVAR EL MIDDLEWARE DE CORS ---
// ¡MUY IMPORTANTE! Debe ir justo aquí: después de app.Build() y ANTES de app.MapControllers()
app.UseCors("PermitirFrontendPython");

//Para authenticacion
app.UseAuthentication();  
app.UseAuthorization();   

// 3. AGREGAR DASHBOARD DE HANGFIRE AQUÍ (Siempre después de UseAuthorization)
app.UseHangfireDashboard("/hangfire");
app.MapControllers();

app.Run();