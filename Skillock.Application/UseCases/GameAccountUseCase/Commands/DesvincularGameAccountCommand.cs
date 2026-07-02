using MediatR;
using Skillock.Application.Common;
using Skillock.Application.Interfaces;
using Skillock.Domain.Common;
using Skillock.Domain.Enums;

namespace Skillock.Application.UseCases.GameAccountUseCase.Commands;

public record DesvincularGameAccountCommand(Guid GameAccountId, Guid UserId)
    : IRequest<ApplicationResult>;

public class DesvincularGameAccountCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<DesvincularGameAccountCommand, ApplicationResult>
{
    public async Task<ApplicationResult> Handle(
        DesvincularGameAccountCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var gameAccount = await unitOfWork.GameAccounts.GetByIdAsync(request.GameAccountId, cancellationToken);
            if (gameAccount is null)
                return ApplicationResult.NotFound("GameAccount", request.GameAccountId);

            if (gameAccount.UserId != request.UserId)
                return ApplicationResult.Forbidden();

            var apuestasActivas = await unitOfWork.Bets.FindAsync(
                b => b.CreatorGameAccountId == request.GameAccountId &&
                     (b.Status == BetStatus.Draft ||
                      b.Status == BetStatus.Negotiating ||
                      b.Status == BetStatus.Agreed ||
                      b.Status == BetStatus.Funding ||
                      b.Status == BetStatus.Active),
                cancellationToken);

            if (apuestasActivas.Count > 0)
                return ApplicationResult.Failure(
                    "ACCOUNT_IN_USE", "No puedes desvincular una cuenta con apuestas activas");

            unitOfWork.GameAccounts.Remove(gameAccount);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return ApplicationResult.Success();
        }
        catch (DomainException ex)
        {
            return ApplicationResult.Failure("DOMAIN_ERROR", ex.Message);
        }
        catch (Exception)
        {
            return ApplicationResult.Failure("ERROR", "Error interno del servidor.");
        }
    }
}
