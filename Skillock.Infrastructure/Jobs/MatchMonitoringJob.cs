using Hangfire;
using MediatR;
using Microsoft.Extensions.Logging;
using Skillock.Application.Interfaces;
using Skillock.Application.UseCases.BetUseCase.Commands;
using Skillock.Domain.Enums;

namespace Skillock.Infrastructure.Jobs;

public class MatchMonitoringJob(
    IUnitOfWork unitOfWork,
    IMediator mediator,
    IEsportApiClientFactory esportApiClientFactory,
    IBackgroundJobClient backgroundJobClient,
    ILogger<MatchMonitoringJob> logger) : IMatchMonitoringJob
{
    public async Task Execute(Guid betId)
    {
        var bet = await unitOfWork.Bets.GetWithPartiesAsync(betId);
        if (bet is null || bet.Status != BetStatus.Active)
            return;

        if (string.IsNullOrEmpty(bet.MatchId))
        {
            backgroundJobClient.Schedule<IMatchMonitoringJob>(
                j => j.Execute(betId), TimeSpan.FromMinutes(2));
            return;
        }

        var resultado = await esportApiClientFactory
            .GetClient(bet.Game)
            .ConsultarResultadoAsync(bet.MatchId);

        if (resultado is null || resultado.Resultado == MatchResult.Pending)
        {
            backgroundJobClient.Schedule<IMatchMonitoringJob>(
                j => j.Execute(betId), TimeSpan.FromMinutes(2));
            return;
        }

        await mediator.Send(new LiquidarApuestaCommand(betId, resultado.Resultado));

        logger.LogInformation("Apuesta {BetId} liquidada con resultado {Resultado}", betId, resultado.Resultado);
    }
}
