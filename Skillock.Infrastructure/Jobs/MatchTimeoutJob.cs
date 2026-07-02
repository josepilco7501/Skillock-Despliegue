using MediatR;
using Microsoft.Extensions.Logging;
using Skillock.Application.Interfaces;
using Skillock.Application.UseCases.UserUseCase.Commands;
using Skillock.Domain.DTOs.Requests;
using Skillock.Domain.Enums;

namespace Skillock.Infrastructure.Jobs;

public class MatchTimeoutJob(
    IUnitOfWork unitOfWork,
    IMediator mediator,
    ILogger<MatchTimeoutJob> logger) : IMatchTimeoutJob
{
    public async Task Execute(Guid betId)
    {
        var bet = await unitOfWork.Bets.GetWithPartiesAsync(betId);
        if (bet is null || bet.Status != BetStatus.Active)
            return;

        await mediator.Send(new ReportarFalloCommand(
            new ReportarFalloRequest(betId, Guid.Empty, "Timeout: partida no resuelta en 4 horas")));

        logger.LogWarning("Apuesta {BetId} en Disputed por timeout de partida", betId);
    }
}
