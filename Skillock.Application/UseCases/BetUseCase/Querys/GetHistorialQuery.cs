using MediatR;
using Skillock.Application.Common;
using Skillock.Application.Interfaces;
using Skillock.Domain.Common;
using Skillock.Domain.DTOs.Requests;
using Skillock.Domain.DTOs.Responses;
using Skillock.Domain.Enums;
using Skillock.Domain.Models;

namespace Skillock.Application.UseCases.BetUseCase.Querys;

public record GetHistorialQuery(Guid UserId, PaginacionRequest Paginacion)
    : IRequest<ApplicationResult<PagedResponse<BetResumenResponse>>>;

public class GetHistorialQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetHistorialQuery, ApplicationResult<PagedResponse<BetResumenResponse>>>
{
    public async Task<ApplicationResult<PagedResponse<BetResumenResponse>>> Handle(
        GetHistorialQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Paginación real en BD — retorna items Y total count en una sola query
            var (bets, totalCount) = await unitOfWork.Bets.GetByUsuarioAsync(
                request.UserId,
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
                ObtenerNombreLiderRival(bet, request.UserId)
            )).ToList().AsReadOnly();

            var totalPaginas = (int)Math.Ceiling((double)totalCount / request.Paginacion.TamanoSeguro);

            return ApplicationResult<PagedResponse<BetResumenResponse>>.Success(
                new PagedResponse<BetResumenResponse>(
                    resumenes,
                    request.Paginacion.Pagina,
                    request.Paginacion.TamanoSeguro,
                    totalCount,       // total real de BD
                    totalPaginas));
        }
        catch (DomainException ex)
        {
            return ApplicationResult<PagedResponse<BetResumenResponse>>
                .Failure("DOMAIN_ERROR", ex.Message);
        }
        catch (Exception)
        {
            return ApplicationResult<PagedResponse<BetResumenResponse>>
                .Failure("ERROR", "Error interno del servidor.");
        }
    }

    private static string? ObtenerNombreLiderRival(Bet bet, Guid userId)
    {
        var partyUsuario = bet.TeamA?.Members.Any(m => m.UserId == userId) == true
            ? bet.TeamA
            : bet.TeamB;

        if (partyUsuario is null) return null;

        var partyRival = partyUsuario.IsTeamA ? bet.TeamB : bet.TeamA;
        return partyRival?.Members
            .FirstOrDefault(m => m.Role == PartyRole.Leader)
            ?.User?.Username;
    }
}
