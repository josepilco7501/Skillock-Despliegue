using MediatR;
using Skillock.Application.Common;
using Skillock.Application.Interfaces;
using Skillock.Domain.Common;
using Skillock.Domain.DTOs.Responses;
using Skillock.Domain.Enums;

namespace Skillock.Application.UseCases.BetUseCase.Commands;

public record ProponerMontoCommand(string CodigoApuesta, decimal MontoPerTeam, Guid LiderId)
    : IRequest<ApplicationResult<BetResponse>>;

public class ProponerMontoCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<ProponerMontoCommand, ApplicationResult<BetResponse>>
{
    public async Task<ApplicationResult<BetResponse>> Handle(ProponerMontoCommand request,
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

            if (request.MontoPerTeam <= 0 || request.MontoPerTeam > DomainConstants.MontoMaximoApuesta)
                return ApplicationResult<BetResponse>.Failure("INVALID_AMOUNT",
                    $"Monto debe estar entre 0 y {DomainConstants.MontoMaximoApuesta}.");

            var partyDelLider =
                bet.TeamA?.Members.Any(m => m.UserId == request.LiderId && m.Role == PartyRole.Leader) == true
                    ? bet.TeamA
                    : bet.TeamB?.Members.Any(m => m.UserId == request.LiderId && m.Role == PartyRole.Leader) == true
                        ? bet.TeamB
                        : null;

            if (partyDelLider is null)
                return ApplicationResult<BetResponse>.Failure("NOT_LEADER", "Usuario no es líder de ningún equipo.");

            bet.AgreedAmountPerTeam = request.MontoPerTeam;
            partyDelLider.LiderAcepto = true;

            var otherParty = partyDelLider.IsTeamA ? bet.TeamB : bet.TeamA;
            if (otherParty is not null)
                otherParty.LiderAcepto = false;

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