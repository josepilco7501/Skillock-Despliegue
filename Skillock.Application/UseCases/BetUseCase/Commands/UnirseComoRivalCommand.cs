using MediatR;
using Skillock.Application.Common;
using Skillock.Application.Interfaces;
using Skillock.Domain.Common;
using Skillock.Domain.DTOs.Requests;
using Skillock.Domain.DTOs.Responses;
using Skillock.Domain.Enums;
using Skillock.Domain.Models;

namespace Skillock.Application.UseCases.BetUseCase.Commands;

public record UnirseComoRivalCommand(string CodigoApuesta, UnirseComoRivalRequest Request, Guid LiderRivalId)
    : IRequest<ApplicationResult<BetResponse>>;

public class UnirseComoRivalCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<UnirseComoRivalCommand, ApplicationResult<BetResponse>>
{
    public async Task<ApplicationResult<BetResponse>> Handle(UnirseComoRivalCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var req = request.Request;

            var bet = await unitOfWork.Bets.GetWithPartiesByMatchIdAsync(request.CodigoApuesta, cancellationToken);
            if (bet is null)
                return ApplicationResult<BetResponse>.Failure("NOT_FOUND", $"No se encontró la apuesta con código {request.CodigoApuesta}.");

            if (DateTime.UtcNow > bet.ExpiresAt)
                return ApplicationResult<BetResponse>.Failure("BET_EXPIRED", "La apuesta ha expirado.");

            if (bet.Status != BetStatus.Draft)
                return ApplicationResult<BetResponse>.Failure("INVALID_STATUS", "La apuesta no está en estado Draft.");

            if (req.TeamSizeB != 1 && req.TeamSizeB != 3 && req.TeamSizeB != 5)
                return ApplicationResult<BetResponse>.Failure("INVALID_TEAM_SIZE", "TeamSize debe ser 1, 3 o 5.");

            var gameAccount = (await unitOfWork.GameAccounts.FindAsync(
                    ga => ga.UserId == request.LiderRivalId && ga.Game == bet.Game && ga.IsActive && ga.VerificadoEn != null,
                    cancellationToken))
                .FirstOrDefault();
            if (gameAccount is null)
                return ApplicationResult<BetResponse>.Failure("INVALID_GAME_ACCOUNT",
                    "No tienes una GameAccount activa y verificada para este juego.");

            // La cuenta se toma de sesión y debe corresponder al mismo juego.
            if (gameAccount.Game != bet.Game)
                return ApplicationResult<BetResponse>.Failure("INVALID_GAME_ACCOUNT",
                    "La GameAccount no corresponde al mismo juego de la apuesta.");

            var tieneApuestaActiva =
                await unitOfWork.Bets.UsuarioTieneApuestaActivaAsync(request.LiderRivalId, bet.Game, cancellationToken);
            if (tieneApuestaActiva)
                return ApplicationResult<BetResponse>.Failure("ACTIVE_BET_EXISTS",
                    "Usuario ya tiene una apuesta activa para este juego.");

            Bet.ValidarCombinacionEquipos(bet.TeamA?.TeamSize ?? 1, req.TeamSizeB);

            var teamB = new BetParty(req.TeamSizeB)
            {
                BetId = bet.Id,
                IsTeamA = false,
                FundingMode = FundingMode.Individual
            };

            var rivalLeader = new PartyMember(teamB.Id, request.LiderRivalId)
            {
                Role = PartyRole.Leader,
                MontoAportado = 0,
                AporteConfirmado = false
            };

            teamB.Members.Add(rivalLeader); // ok, sigue siendo parte del grafo nuevo
            bet.BetParties.Add(teamB);      // mantiene la navegación en memoria (útil para el mapper)
            bet.Status = BetStatus.Negotiating;

            await unitOfWork.BetParties.AddAsync(teamB, cancellationToken); // 👈 esto marca Added explícitamente y cascada a rivalLeader
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return ApplicationResult<BetResponse>.Success(BetMapper.ToResponse(bet));
        }
        catch (DomainException ex)
        {
            return ApplicationResult<BetResponse>.Failure("DOMAIN_ERROR", ex.Message);
        }
        catch (Exception ex)
        {
            return ApplicationResult<BetResponse>.Failure("ERROR", $"DEBUG: {ex.Message} | {ex.InnerException?.Message}");
        }
    }
}