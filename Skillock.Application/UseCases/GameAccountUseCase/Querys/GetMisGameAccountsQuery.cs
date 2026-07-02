using MediatR;
using Skillock.Application.Common;
using Skillock.Application.Interfaces;
using Skillock.Domain.Common;
using Skillock.Domain.DTOs.Responses;
using Skillock.Domain.Models;

namespace Skillock.Application.UseCases.GameAccountUseCase.Querys;

public record GetMisGameAccountsQuery(Guid UserId)
    : IRequest<ApplicationResult<IReadOnlyList<GameAccountResponse>>>;

public class GetMisGameAccountsQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetMisGameAccountsQuery, ApplicationResult<IReadOnlyList<GameAccountResponse>>>
{
    public async Task<ApplicationResult<IReadOnlyList<GameAccountResponse>>> Handle(
        GetMisGameAccountsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var cuentas = await unitOfWork.GameAccounts.FindAsync(
                ga => ga.UserId == request.UserId, cancellationToken);

            var response = cuentas.Select(ToResponse).ToList().AsReadOnly();
            return ApplicationResult<IReadOnlyList<GameAccountResponse>>.Success(response);
        }
        catch (DomainException ex)
        {
            return ApplicationResult<IReadOnlyList<GameAccountResponse>>.Failure("DOMAIN_ERROR", ex.Message);
        }
        catch (Exception)
        {
            return ApplicationResult<IReadOnlyList<GameAccountResponse>>.Failure("ERROR", "Error interno del servidor.");
        }
    }

    private static GameAccountResponse ToResponse(GameAccount ga) =>
        new(ga.Id, ga.Game, ga.GamePlayerId, ga.GamePlayerTag, ga.IsActive, ga.VerificadoEn, ga.CreatedAt);
}
