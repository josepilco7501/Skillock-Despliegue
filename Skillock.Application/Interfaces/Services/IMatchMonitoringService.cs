using Skillock.Domain.Enums;

namespace Skillock.Application.Interfaces;

public interface IMatchMonitoringService
{
    /// <summary>
    /// Ejecuta el ciclo de monitoreo para consultar apuestas activas y liquidar las que tengan resultado disponible.
    /// </summary>
    Task EjecutarCicloMonitoreoAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Determina si una partida ya terminó y tiene resultado disponible en la API del juego.
    /// </summary>
    Task<bool> PartidaTerminoAsync(string matchId, EsportGame game, CancellationToken cancellationToken);
}

