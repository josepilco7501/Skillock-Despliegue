using Microsoft.Extensions.Logging;
using Skillock.Application.DTOs;
using Skillock.Application.Interfaces;
using Skillock.Domain.Enums;

namespace Skillock.Infrastructure.Services.EsportApiClients;

public class Dota2ApiClient(HttpClient httpClient, ILogger<Dota2ApiClient> logger) : IEsportApiClient
{
    public EsportGame JuegoSoportado => EsportGame.Dota2;

    public Task<bool> ValidarMatchIdAsync(string matchId, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("TODO: implementar contra API real de Dota2/Steam");
        return Task.FromResult(true);
    }

    public Task<MatchResultDto?> ConsultarResultadoAsync(string matchId, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("TODO: implementar contra API real de Dota2/Steam");
        return Task.FromResult<MatchResultDto?>(null);
    }

    public Task<DateTime?> ObtenerMatchStartedAtAsync(string matchId, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("TODO: implementar contra API real de Dota2/Steam");
        return Task.FromResult<DateTime?>(DateTime.UtcNow.AddHours(-1));
    }
}

