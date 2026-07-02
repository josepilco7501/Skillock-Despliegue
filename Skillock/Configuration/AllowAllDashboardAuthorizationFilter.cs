using Hangfire.Dashboard;

namespace Skillock_ProyectoFinal.Configuration;

// Para evitar usar JWT en el header — SOLO PARA DESARROLLO
public class AllowAllDashboardAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context) => true;
}