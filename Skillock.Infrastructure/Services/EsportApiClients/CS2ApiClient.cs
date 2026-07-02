using Microsoft.Extensions.Logging;
using Skillock.Application.DTOs;
using Skillock.Application.Interfaces;
using Skillock.Domain.Enums;

namespace Skillock.Infrastructure.Services.EsportApiClients;

public class CS2ApiClient(HttpClient httpClient, ILogger<CS2ApiClient> logger) : IEsportApiClient
{
    public EsportGame JuegoSoportado => EsportGame.CS2;

    public Task<bool> ValidarMatchIdAsync(string matchId, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("TODO: implementar contra API real de CS2");
        return Task.FromResult(true);
    }

    public Task<MatchResultDto?> ConsultarResultadoAsync(string matchId, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("TODO: implementar contra API real de CS2");
        return Task.FromResult<MatchResultDto?>(null);
    }

    public Task<DateTime?> ObtenerMatchStartedAtAsync(string matchId, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("TODO: implementar contra API real de CS2");
        return Task.FromResult<DateTime?>(DateTime.UtcNow.AddHours(-1));
    }
}

