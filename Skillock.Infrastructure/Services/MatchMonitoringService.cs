using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Skillock.Application.Interfaces;
using Skillock.Application.UseCases.BetUseCase.Commands;
using Skillock.Domain.Enums;

namespace Skillock.Infrastructure.Services;

public class MatchMonitoringService(IServiceScopeFactory scopeFactory, ILogger<MatchMonitoringService> logger) : IMatchMonitoringService
{
    public async Task EjecutarCicloMonitoreoAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var clientFactory = scope.ServiceProvider.GetRequiredService<IEsportApiClientFactory>();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var bets = await unitOfWork.Bets.GetActivasParaMonitoreoAsync(cancellationToken);

        await Parallel.ForEachAsync(bets, new ParallelOptions { MaxDegreeOfParallelism = 3, CancellationToken = cancellationToken }, async (bet, ct) =>
        {
            try
            {
                if (string.IsNullOrWhiteSpace(bet.MatchId))
                    return;

                var client = clientFactory.GetClient(bet.Game);
                var resultado = await client.ConsultarResultadoAsync(bet.MatchId, ct);

                logger.LogInformation("Monitoreo de apuesta {BetId}: Match {MatchId} resultado {Resultado}", bet.Id, bet.MatchId, resultado?.Resultado.ToString() ?? "null");

                if (resultado is not null && resultado.Resultado != MatchResult.Pending)
                    await mediator.Send(new LiquidarApuestaCommand(bet.Id, resultado.Resultado), ct);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error monitoreando apuesta {BetId}", bet.Id);
            }
        });
    }

    public async Task<bool> PartidaTerminoAsync(string matchId, EsportGame game, CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var clientFactory = scope.ServiceProvider.GetRequiredService<IEsportApiClientFactory>();
        var client = clientFactory.GetClient(game);
        var resultado = await client.ConsultarResultadoAsync(matchId, cancellationToken);
        return resultado is not null && resultado.Resultado != MatchResult.Pending;
    }
}

