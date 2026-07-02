using Skillock.Application.DTOs;
using Skillock.Domain.Enums;

namespace Skillock.Application.Interfaces;

public interface IEsportApiClient
{
    EsportGame JuegoSoportado { get; }
    Task<bool> ValidarMatchIdAsync(string matchId, CancellationToken cancellationToken = default);
    Task<MatchResultDto?> ConsultarResultadoAsync(string matchId, CancellationToken cancellationToken = default);
    Task<DateTime?> ObtenerMatchStartedAtAsync(string matchId, CancellationToken cancellationToken = default);
}

