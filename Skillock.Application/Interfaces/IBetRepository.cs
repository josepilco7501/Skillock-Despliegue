using GamerBet.Domain.Entities;
using GamerBet.Domain.Enums;

namespace GamerBet.Application.Interfaces;

/// <summary>
/// Repositorio especializado para el Aggregate Root Bet.
/// Los métodos con nombre aquí reemplazan queries IQueryable en los servicios,
/// manteniendo la lógica de acceso a datos en Infrastructure.
/// </summary>
public interface IBetRepository : IRepository<Bet>
{
    /// <summary>
    /// Carga la apuesta con sus dos parties y todos sus miembros.
    /// Es la query más frecuente: casi toda operación de negocio la necesita.
    /// </summary>
    Task<Bet?> GetWithPartiesAsync(Guid betId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Todas las apuestas en estado Active con MatchId asignado.
    /// Usada por el BackgroundService de monitoreo en cada ciclo de polling.
    /// </summary>
    Task<IReadOnlyList<Bet>> GetActivasParaMonitoreoAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Historial de apuestas de un usuario (como miembro de cualquier party).
    /// </summary>
    Task<IReadOnlyList<Bet>> GetByUsuarioAsync(
        Guid userId,
        int pagina,
        int tamano,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Apuestas en estado Funding que llevan más tiempo del timeout configurado
    /// sin completar el fondeo. El BackgroundService las cancela automáticamente.
    /// </summary>
    Task<IReadOnlyList<Bet>> GetFundingExpiradasAsync(
        TimeSpan timeout,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica si un usuario ya es miembro de una apuesta activa para el mismo juego.
    /// Regla de negocio: un usuario no puede tener dos apuestas activas simultáneas
    /// en el mismo juego (evita conflictos de MatchId).
    /// </summary>
    Task<bool> UsuarioTieneApuestaActivaAsync(
        Guid userId,
        EsportGame game,
        CancellationToken cancellationToken = default);
}
