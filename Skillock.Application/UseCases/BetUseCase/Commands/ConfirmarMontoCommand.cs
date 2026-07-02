    using Hangfire;
using MediatR;
using Microsoft.Extensions.Logging;
using Skillock.Application.Common;
using Skillock.Application.Interfaces;
using Skillock.Domain.Common;
using Skillock.Domain.DTOs.Responses;
using Skillock.Domain.Enums;
// using Skillock.Infrastructure.Jobs; // no referencia directa a Infrastructure desde Application

namespace Skillock.Application.UseCases.BetUseCase.Commands;

public record ConfirmarMontoCommand(string CodigoApuesta, Guid LiderId)
    : IRequest<ApplicationResult<BetResponse>>;

public class ConfirmarMontoCommandHandler(
    IUnitOfWork unitOfWork,
    IBackgroundJobClient backgroundJobClient,
    ILogger<ConfirmarMontoCommandHandler> logger)
    : IRequestHandler<ConfirmarMontoCommand, ApplicationResult<BetResponse>>
{
    public async Task<ApplicationResult<BetResponse>> Handle(ConfirmarMontoCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var bet = await unitOfWork.Bets.GetWithPartiesByMatchIdAsync(request.CodigoApuesta, cancellationToken);
            if (bet is null)
                return ApplicationResult<BetResponse>.Failure("NOT_FOUND", $"No se encontró la apuesta con código {request.CodigoApuesta}.");

            if (DateTime.UtcNow > bet.ExpiresAt)
                return ApplicationResult<BetResponse>.Failure("BET_EXPIRED", "La apuesta ha expirado.");

            if (bet.Status != BetStatus.Negotiating)
                return ApplicationResult<BetResponse>.Failure("INVALID_STATUS",
                    "La apuesta no está en estado Negotiating.");

            var partyDelLider =
                bet.TeamA?.Members.Any(m => m.UserId == request.LiderId && m.Role == PartyRole.Leader) == true
                    ? bet.TeamA
                    : bet.TeamB?.Members.Any(m => m.UserId == request.LiderId && m.Role == PartyRole.Leader) == true
                        ? bet.TeamB
                        : null;

            if (partyDelLider is null)
                return ApplicationResult<BetResponse>.Failure("NOT_LEADER", "Usuario no es líder de ningún equipo.");

            partyDelLider.LiderAcepto = true;

            // Si ambos líderes aceptan, pasar a Agreed y ACTIVAR inmediatamente.
            var activada = false;
            if (bet.TeamA?.LiderAcepto == true && bet.TeamB?.LiderAcepto == true)
            {
                // Marcar acordada y activada
                bet.Status = BetStatus.Agreed;
                bet.Status = BetStatus.Active; // Activar inmediatamente
                bet.ActivatedAt = DateTime.UtcNow;
                activada = true;
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);

            if (activada)
            {
                // Programar jobs de monitoreo / timeout como cuando se activa por fondeo
                backgroundJobClient.Schedule<IMatchMonitoringJob>(
                    j => j.Execute(bet.Id),
                    TimeSpan.FromMinutes(2));
                backgroundJobClient.Schedule<IMatchTimeoutJob>(
                    j => j.Execute(bet.Id),
                    TimeSpan.FromHours(4));
                logger.LogInformation("Apuesta {BetId} activada manualmente por confirmación de monto", bet.Id);
            }

            return ApplicationResult<BetResponse>.Success(BetMapper.ToResponse(bet));
        }
        catch (DomainException ex)
        {
            logger.LogWarning(ex, "Domain error confirming amount for bet {CodigoApuesta}", request.CodigoApuesta);
            return ApplicationResult<BetResponse>.Failure("DOMAIN_ERROR", ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error confirming amount for bet {CodigoApuesta}", request.CodigoApuesta);
            return ApplicationResult<BetResponse>.Failure("ERROR", "Error interno del servidor.");
        }
    }
}