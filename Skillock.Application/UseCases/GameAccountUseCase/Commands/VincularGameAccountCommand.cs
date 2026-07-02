using MediatR;
using Skillock.Application.Common;
using Skillock.Application.Interfaces;
using Skillock.Domain.Common;
using Skillock.Domain.DTOs.Requests;
using Skillock.Domain.DTOs.Responses;
using Skillock.Domain.Models;

namespace Skillock.Application.UseCases.GameAccountUseCase.Commands;

public record VincularGameAccountCommand(VincularGameAccountRequest Request, Guid UserId)
    : IRequest<ApplicationResult<GameAccountResponse>>;

public class VincularGameAccountCommandHandler(
    IUnitOfWork unitOfWork,
    IEsportApiClientFactory esportApiClientFactory)
    : IRequestHandler<VincularGameAccountCommand, ApplicationResult<GameAccountResponse>>
{
    public async Task<ApplicationResult<GameAccountResponse>> Handle(
        VincularGameAccountCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var req = request.Request;

            var existentePorJuego = await unitOfWork.GameAccounts.FindAsync(
                ga => ga.UserId == request.UserId && ga.Game == req.Game, cancellationToken);
            if (existentePorJuego.Count > 0)
                return ApplicationResult<GameAccountResponse>.Failure(
                    "GAME_ACCOUNT_EXISTS", "Ya tienes una cuenta vinculada para este juego");

            var cuentasUsuario = await unitOfWork.GameAccounts.FindAsync(
                ga => ga.UserId == request.UserId, cancellationToken);
            if (cuentasUsuario.Count >= 3)
                return ApplicationResult<GameAccountResponse>.Failure(
                    "MAX_ACCOUNTS_REACHED", "Máximo 3 cuentas de juego vinculadas");

            var esValido = await esportApiClientFactory
                .GetClient(req.Game)
                .ValidarMatchIdAsync(req.GamePlayerId, cancellationToken);
            if (!esValido)
                return ApplicationResult<GameAccountResponse>.Failure(
                    "INVALID_PLAYER_ID", "El ID de jugador no es válido en la plataforma del juego");

            var gameAccount = new GameAccount(
                request.UserId,
                req.Game,
                req.GamePlayerId,
                req.GamePlayerTag,
                true,
                DateTime.UtcNow);

            await unitOfWork.GameAccounts.AddAsync(gameAccount, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return ApplicationResult<GameAccountResponse>.Success(ToResponse(gameAccount));
        }
        catch (DomainException ex)
        {
            return ApplicationResult<GameAccountResponse>.Failure("DOMAIN_ERROR", ex.Message);
        }
        catch (Exception ex)
        {
            return ApplicationResult<GameAccountResponse>.Failure("ERROR", $"DEBUG: {ex.Message} | {ex.InnerException?.Message}");
        }
    }

    private static GameAccountResponse ToResponse(GameAccount ga) =>
        new(ga.Id, ga.Game, ga.GamePlayerId, ga.GamePlayerTag, ga.IsActive, ga.VerificadoEn, ga.CreatedAt);
}
