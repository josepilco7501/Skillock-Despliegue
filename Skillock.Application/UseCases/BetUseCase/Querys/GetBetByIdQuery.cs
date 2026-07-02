using MediatR;
using Skillock.Application.Common;
using Skillock.Application.Interfaces;
using Skillock.Domain.Common;
using Skillock.Domain.DTOs.Responses;

namespace Skillock.Application.UseCases.BetUseCase.Querys;

public record GetBetByIdQuery(Guid BetId, Guid UserId)
    : IRequest<ApplicationResult<BetResponse>>;

public class GetBetByIdQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetBetByIdQuery, ApplicationResult<BetResponse>>
{
    public async Task<ApplicationResult<BetResponse>> Handle(GetBetByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var bet = await unitOfWork.Bets.GetWithPartiesAsync(request.BetId, cancellationToken);
            if (bet is null)
                return ApplicationResult<BetResponse>.NotFound("Bet", request.BetId);

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