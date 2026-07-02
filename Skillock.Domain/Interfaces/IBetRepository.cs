using Skillock.Domain.Enums;
using Skillock.Domain.Models;

namespace Skillock.Domain.Interfaces;

public interface IBetRepository : IRepository<Bet>
{
    Task<Bet?> GetWithPartiesAsync(Guid betId, CancellationToken cancellationToken = default);
    Task<Bet?> GetWithPartiesByMatchIdAsync(string matchId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Bet>> GetActivasParaMonitoreoAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retorna items paginados Y el total real de registros para paginación correcta en BD.
    /// </summary>
    Task<(IReadOnlyList<Bet> Items, int TotalCount)> GetByUsuarioAsync(
        Guid userId, int pagina, int tamano, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<Bet> Items, int TotalCount)> GetByStatusAsync(
        BetStatus status, int pagina, int tamano, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Bet>> GetFundingExpiradasAsync(DateTime ahora, CancellationToken cancellationToken = default);
    Task<bool> UsuarioTieneApuestaActivaAsync(Guid userId, EsportGame game, CancellationToken cancellationToken = default);
}
