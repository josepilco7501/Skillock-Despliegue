using MediatR;
using Skillock.Application.Common;
using Skillock.Application.Interfaces;
using Skillock.Domain.Common;
using Skillock.Domain.DTOs.Responses;
using Skillock.Domain.Enums;

namespace Skillock.Application.UseCases.BetUseCase.Commands;

public record ConfirmarMontoCommand(string CodigoApuesta, Guid LiderId)
    : IRequest<ApplicationResult<BetResponse>>;

public class ConfirmarMontoCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<ConfirmarMontoCommand, ApplicationResult<BetResponse>>
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

            if (bet.TeamA?.LiderAcepto == true && bet.TeamB?.LiderAcepto == true)
                bet.Status = BetStatus.Agreed;

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return ApplicationResult<BetResponse>.Success(BetMapper.ToResponse(bet));
        }
        catch (DomainException ex)
        {
            return ApplicationResult<BetResponse>.Failure("DOMAIN_ERROR", ex.Message);
        }
        catch (Exception ex)
        {
            return ApplicationResult<BetResponse>.Failure("ERROR", "Error interno del servidor.");
        }
    }
}