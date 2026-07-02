using MediatR;
using Skillock.Application.Common;
using Skillock.Domain.Common;
using Skillock.Application.Interfaces;
using Skillock.Domain.DTOs.Requests;
using Skillock.Domain.DTOs.Responses;
using Skillock.Domain.Enums;
using Skillock.Domain.Models;

namespace Skillock.Application.UseCases.BetUseCase.Querys;

public record GetCompletedBetsQuery(PaginacionRequest Paginacion)
    : IRequest<ApplicationResult<PagedResponse<BetResumenResponse>>>;

public class GetCompletedBetsQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetCompletedBetsQuery, ApplicationResult<PagedResponse<BetResumenResponse>>>
{
    public async Task<ApplicationResult<PagedResponse<BetResumenResponse>>> Handle(
        GetCompletedBetsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var (bets, totalCount) = await unitOfWork.Bets.GetByStatusAsync(
                BetStatus.Completed,
                request.Paginacion.Pagina,
                request.Paginacion.TamanoSeguro,
                cancellationToken);

            var resumenes = bets.Select(bet => new BetResumenResponse(
                bet.Id,
                bet.Game,
                bet.Status,
                bet.AgreedAmountPerTeam,
                bet.PremioNeto,
                bet.MatchId,
                bet.CreatedAt,
                bet.CompletedAt,
                ObtenerLiderRival(bet)
            )).ToList().AsReadOnly();

            var totalPaginas = (int)Math.Ceiling((double)totalCount / request.Paginacion.TamanoSeguro);

            return ApplicationResult<PagedResponse<BetResumenResponse>>.Success(
                new PagedResponse<BetResumenResponse>(
                    resumenes,
                    request.Paginacion.Pagina,
                    request.Paginacion.TamanoSeguro,
                    totalCount,
                    totalPaginas));
        }
        catch (DomainException ex)
        {
            return ApplicationResult<PagedResponse<BetResumenResponse>>.Failure("DOMAIN_ERROR", ex.Message);
        }
        catch (Exception)
        {
            return ApplicationResult<PagedResponse<BetResumenResponse>>.Failure("ERROR", "Error interno del servidor.");
        }
    }

    private static string? ObtenerLiderRival(Bet bet)
    {
        // Para el administrador devolvemos el líder del equipo B (si existe),
        // en caso de querer otro comportamiento se puede ajustar.
        var partyRival = bet.TeamB ?? bet.TeamA;
        return partyRival?.Members.FirstOrDefault(m => m.Role == PartyRole.Leader)?.User?.Username;
    }
}


