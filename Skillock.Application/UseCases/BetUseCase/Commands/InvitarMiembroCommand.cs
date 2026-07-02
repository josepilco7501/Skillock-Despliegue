using MediatR;
using Skillock.Application.Common;
using Skillock.Application.Interfaces;
using Skillock.Domain.Common;
using Skillock.Domain.DTOs.Requests;
using Skillock.Domain.DTOs.Responses;
using Skillock.Domain.Enums;
using Skillock.Domain.Models;

namespace Skillock.Application.UseCases.BetUseCase.Commands;

public record InvitarMiembroCommand(InvitarMiembroRequest Request)
    : IRequest<ApplicationResult<BetResponse>>;

public class InvitarMiembroCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<InvitarMiembroCommand, ApplicationResult<BetResponse>>
{
    public async Task<ApplicationResult<BetResponse>> Handle(
        InvitarMiembroCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var req = request.Request;

            var bet = await unitOfWork.Bets.GetWithPartiesAsync(req.BetId, cancellationToken);
            if (bet is null)
                return ApplicationResult<BetResponse>.NotFound("Bet", req.BetId);

            if (DateTime.UtcNow > bet.ExpiresAt)
                return ApplicationResult<BetResponse>.Failure("BET_EXPIRED", "La apuesta ha expirado.");

            if (bet.Status != BetStatus.Agreed && bet.Status != BetStatus.Funding)
                return ApplicationResult<BetResponse>.Failure(
                    "INVALID_STATUS", "La apuesta debe estar en Agreed o Funding.");

            var party = bet.BetParties.FirstOrDefault(bp => bp.Id == req.BetPartyId);
            if (party is null)
                return ApplicationResult<BetResponse>.NotFound("BetParty", req.BetPartyId);

            if (party.Members.All(m => m.UserId != req.LiderId || m.Role != PartyRole.Leader))
                return ApplicationResult<BetResponse>.Failure(
                    "NOT_LEADER", "No eres líder de este equipo.");

            if (party.FundingMode != FundingMode.Mutual)
                return ApplicationResult<BetResponse>.Failure(
                    "NOT_MUTUAL", "Solo Fondo Mutuo permite invitar miembros.");

            if (party.Members.Count >= party.TeamSize)
                return ApplicationResult<BetResponse>.Failure(
                    "TEAM_FULL", "El equipo ya está completo.");

            if (party.Members.Any(m => m.UserId == req.UsuarioInvitadoId))
                return ApplicationResult<BetResponse>.Failure(
                    "ALREADY_MEMBER", "El usuario ya es miembro del equipo.");

            var nuevoMiembro = new PartyMember(party.Id, req.UsuarioInvitadoId)
            {
                Role = PartyRole.Member,
                AporteConfirmado = false,
                MontoAportado = 0
            };

            party.Members.Add(nuevoMiembro);
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
