using MediatR;
using Skillock.Application.Common;
using Skillock.Application.Interfaces;
using Skillock.Domain.Common;
using Skillock.Domain.DTOs.Responses;
using Skillock.Domain.Enums;

namespace Skillock.Application.UseCases.BetUseCase.Commands;

public record ElegirModalidadFondeoCommand(string CodigoApuesta, FundingMode Modalidad, Guid LiderId)
    : IRequest<ApplicationResult<BetResponse>>;

public class ElegirModalidadFondeoCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<ElegirModalidadFondeoCommand, ApplicationResult<BetResponse>>
{
    public async Task<ApplicationResult<BetResponse>> Handle(
        ElegirModalidadFondeoCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var bet = await unitOfWork.Bets.GetWithPartiesByMatchIdAsync(request.CodigoApuesta, cancellationToken);
            if (bet is null)
                return ApplicationResult<BetResponse>.Failure("NOT_FOUND", $"No se encontró la apuesta con código {request.CodigoApuesta}.");

            if (DateTime.UtcNow > bet.ExpiresAt)
                return ApplicationResult<BetResponse>.Failure("BET_EXPIRED", "La apuesta ha expirado.");

            if (bet.Status != BetStatus.Agreed)
                return ApplicationResult<BetResponse>.Failure(
                    "INVALID_STATUS", "La apuesta debe estar en estado Agreed.");

            // Identificar la party del líder
            var partyDelLider =
                bet.TeamA?.Members.Any(m => m.UserId == request.LiderId && m.Role == PartyRole.Leader) == true
                    ? bet.TeamA
                    : bet.TeamB?.Members.Any(m => m.UserId == request.LiderId && m.Role == PartyRole.Leader) == true
                        ? bet.TeamB
                        : null;

            if (partyDelLider is null)
                return ApplicationResult<BetResponse>.Failure(
                    "NOT_LEADER", "No eres líder de ningún equipo en esta apuesta.");

            partyDelLider.FundingMode = request.Modalidad;
            partyDelLider.ModalidadElegida = true; // flag explícito, no depende del valor del enum

            // Transicionar a Funding solo cuando AMBOS líderes eligieron su modalidad
            if (bet.TeamA?.ModalidadElegida == true && bet.TeamB?.ModalidadElegida == true)
                bet.Status = BetStatus.Funding;

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return ApplicationResult<BetResponse>.Success(BetMapper.ToResponse(bet));
        }
        catch (DomainException ex)
        {
            return ApplicationResult<BetResponse>.Failure("DOMAIN_ERROR", ex.Message);
        }
        catch (Exception)
        {
            return ApplicationResult<BetResponse>.Failure("ERROR", "Error interno del servidor.");
        }
    }
}
