using MediatR;
using Skillock.Application.Common;
using Skillock.Application.Interfaces;
using Skillock.Domain.Common;
using Skillock.Domain.DTOs.Requests;
using Skillock.Domain.Enums;

namespace Skillock.Application.UseCases.UserUseCase.Commands;

public record ReportarFalloCommand(ReportarFalloRequest Request)
    : IRequest<ApplicationResult>;

public class ReportarFalloCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<ReportarFalloCommand, ApplicationResult>
{
    public async Task<ApplicationResult> Handle(ReportarFalloCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var req = request.Request;

            var bet = await unitOfWork.Bets.GetWithPartiesAsync(req.BetId, cancellationToken);
            if (bet is null)
                return ApplicationResult.NotFound("Bet", req.BetId);

            if (bet.Status != BetStatus.Active)
                return ApplicationResult.Failure("INVALID_STATUS", "La apuesta no está en estado Active.");

            var esMiembro = bet.BetParties.Any(p => p.Members.Any(m => m.UserId == req.SolicitanteId));
            if (!esMiembro)
                return ApplicationResult.Forbidden();

            bet.Status = BetStatus.Disputed;
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return ApplicationResult.Success();
        }
        catch (DomainException ex)
        {
            return ApplicationResult.Failure("DOMAIN_ERROR", ex.Message);
        }
        catch (Exception ex)
        {
            return ApplicationResult.Failure("ERROR", "Error interno del servidor.");
        }
    }
}

