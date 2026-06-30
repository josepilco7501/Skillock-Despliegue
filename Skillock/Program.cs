using Hangfire;
using Skillock_ProyectoFinal.Configuration;

var builder = WebApplication.CreateBuilder(args);

// 1. Centraliza la inyección de dependencias usando el método de tu profesor
builder.Services.AddApplicationServices(builder.Configuration);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
//Para authenticacion
app.UseAuthentication();  // ← esta línea
app.UseAuthorization();   // ← esta línea
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new HangfireAdminAuthorizationFilter() }
});
app.MapControllers();

app.Run();