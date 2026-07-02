namespace Skillock.Application.Interfaces;

public interface IBetExpirationService
{
    /// <summary>
    /// Ejecuta el ciclo de expiración para cancelar automáticamente apuestas vencidas y liberar fondos retenidos.
    /// </summary>
    Task EjecutarCicloExpiracionAsync(CancellationToken cancellationToken);
}

