using Hangfire;
using MediatR;
using Microsoft.Extensions.Logging;
using Skillock.Application.Common;
using Skillock.Application.Interfaces;
using Skillock.Domain.Common;
using Skillock.Domain.DTOs.Requests;
using Skillock.Domain.DTOs.Responses;
using Skillock.Domain.Enums;
using Skillock.Domain.Models;

namespace Skillock.Application.UseCases.BetUseCase.Commands;

public record CrearApuestaCommand(CrearApuestaRequest Request, Guid LiderId)
    : IRequest<ApplicationResult<BetResponse>>;

public class CrearApuestaCommandHandler(
    IUnitOfWork unitOfWork,
    IBackgroundJobClient backgroundJobClient,
    ILogger<CrearApuestaCommandHandler> logger)
    : IRequestHandler<CrearApuestaCommand, ApplicationResult<BetResponse>>
{
    public async Task<ApplicationResult<BetResponse>> Handle(
        CrearApuestaCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var req = request.Request;

            // Validar TeamSize
            if (req.TeamSizeA != 1 && req.TeamSizeA != 3 && req.TeamSizeA != 5)
                return ApplicationResult<BetResponse>.Failure(
                    "INVALID_TEAM_SIZE", "TeamSize debe ser 1, 3 o 5.");

            // Validar monto
            if (req.MontoInicial <= 0 || req.MontoInicial > DomainConstants.MontoMaximoApuesta)
                return ApplicationResult<BetResponse>.Failure(
                    "INVALID_AMOUNT", $"Monto debe estar entre 0 y S/.{DomainConstants.MontoMaximoApuesta}.");

            // Validar GameAccount
            var gameAccount = await unitOfWork.GameAccounts.GetByIdAsync(req.GameAccountId, cancellationToken);
            if (gameAccount is null)
                return ApplicationResult<BetResponse>.NotFound("GameAccount", req.GameAccountId);

            if (gameAccount.UserId != request.LiderId
                || gameAccount.Game != req.Game
                || !gameAccount.IsActive
                || gameAccount.VerificadoEn is null)
                return ApplicationResult<BetResponse>.Failure(
                    "INVALID_GAME_ACCOUNT", "La GameAccount no es válida para crear la apuesta.");

            // Validar saldo disponible antes de crear la apuesta
            var wallet = await unitOfWork.Wallets.GetByUserIdWithLockAsync(request.LiderId, cancellationToken);
            if (wallet is null)
                return ApplicationResult<BetResponse>.Failure("NO_WALLET", "El usuario no tiene wallet.");

            if (wallet.SaldoDisponible < req.MontoInicial)
                return ApplicationResult<BetResponse>.Failure(
                    "INSUFFICIENT_BALANCE",
                    "Saldo insuficiente para crear la apuesta.");

            // Validar que no tenga apuesta activa en el mismo juego
            var tieneApuestaActiva = await unitOfWork.Bets
                .UsuarioTieneApuestaActivaAsync(request.LiderId, req.Game, cancellationToken);
            if (tieneApuestaActiva)
                return ApplicationResult<BetResponse>.Failure(
                    "ACTIVE_BET_EXISTS", "Ya tienes una apuesta activa para este juego.");

            // Crear apuesta
            var bet = new Bet(req.Game, DomainConstants.PlatformFeePercent, req.GameAccountId)
            {
                MatchId = req.MatchId,
                MatchStartedAt = req.MatchStartedAt,
                Status = BetStatus.Draft,
                AgreedAmountPerTeam = req.MontoInicial  // propuesta inicial del creador
            };

            // Validar que la partida ya esté en curso
            if (req.MatchStartedAt >= bet.CreatedAt)
                return ApplicationResult<BetResponse>.Failure(
                    "INVALID_MATCH_TIME",
                    "La partida debe haber iniciado antes de crear la apuesta.");

            // Crear TeamA con el líder
            var teamA = new BetParty(req.TeamSizeA)
            {
                BetId = bet.Id,
                IsTeamA = true,
                FundingMode = FundingMode.Individual,
                ModalidadElegida = false
            };

            var leaderMember = new PartyMember(teamA.Id, request.LiderId)
            {
                Role = PartyRole.Leader,
                MontoAportado = 0,
                AporteConfirmado = false
            };

            teamA.Members.Add(leaderMember);
            bet.BetParties.Add(teamA);

            await unitOfWork.Bets.AddAsync(bet, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            // En CrearApuestaCommand.cs — reemplaza el TimeSpan fijo:
            var delay = bet.ExpiresAt - DateTime.UtcNow;

            backgroundJobClient.Schedule<IBetExpirationJob>(
                j => j.Execute(bet.Id),
                delay);  // ← se deriva directamente de ExpiresAt, nunca se desincroniza
            logger.LogInformation("BetExpirationJob programado para apuesta {BetId}, expira en {Delay}", bet.Id, delay);

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
