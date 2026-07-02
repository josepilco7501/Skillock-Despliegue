using MediatR;
using Microsoft.Extensions.Logging;
using Skillock.Application.Interfaces;
using Skillock.Application.UseCases.BetUseCase.Commands;
using Skillock.Domain.DTOs.Requests;
using Skillock.Domain.Enums;

namespace Skillock.Infrastructure.Jobs;

public class BetExpirationJob(
    IUnitOfWork unitOfWork,
    IMediator mediator,
    ILogger<BetExpirationJob> logger) : IBetExpirationJob
{
    public async Task Execute(Guid betId)
    {
        logger.LogInformation("BetExpirationJob ejecutando para {BetId}", betId);
    
        var bet = await unitOfWork.Bets.GetWithPartiesAsync(betId);
        if (bet is null)
        {
            logger.LogWarning("Apuesta {BetId} no encontrada", betId);
            return;
        }

        logger.LogInformation("Apuesta {BetId} tiene Status={Status}, ExpiresAt={ExpiresAt}, UtcNow={Now}", 
            betId, bet.Status, bet.ExpiresAt, DateTime.UtcNow);

        if (bet.Status is BetStatus.Active or BetStatus.Completed or BetStatus.Cancelled)
        {
            logger.LogInformation("Apuesta {BetId} ya está en estado final {Status}, no se cancela", betId, bet.Status);
            return;
        }

        var resultado = await mediator.Send(new CancelarApuestaCommand(
            new CancelarApuestaRequest(betId, Guid.Empty, "Expiración automática de 5 minutos")));

        if (!resultado.IsSuccess)
            logger.LogError("Fallo cancelar {BetId}: {Code} - {Msg}", betId, resultado.ErrorCode, resultado.ErrorMessage);
        else
            logger.LogInformation("Apuesta {BetId} cancelada exitosamente", betId);
    }
}
