using MediatR;
using Skillock.Application.Common;
using Skillock.Application.Interfaces;
using Skillock.Domain.Common;
using Skillock.Domain.DTOs.Responses;
using Skillock.Domain.Enums;
using Skillock.Domain.Models;

namespace Skillock.Application.UseCases.BetUseCase.Querys;

public record GetEstadoFondeoQuery(Guid BetId)
    : IRequest<ApplicationResult<FondeoEstadoResponse>>;

public class GetEstadoFondeoQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetEstadoFondeoQuery, ApplicationResult<FondeoEstadoResponse>>
{
    public async Task<ApplicationResult<FondeoEstadoResponse>> Handle(GetEstadoFondeoQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var bet = await unitOfWork.Bets.GetWithPartiesAsync(request.BetId, cancellationToken);
            if (bet is null)
                return ApplicationResult<FondeoEstadoResponse>.NotFound("Bet", request.BetId);

            if (bet.Status != BetStatus.Funding)
                return ApplicationResult<FondeoEstadoResponse>.Failure("INVALID_STATUS", "La apuesta no está en estado Funding.");

            var agreedAmount = bet.AgreedAmountPerTeam ?? 0;
            var teamAResponse = MapFondeoEquipoResponse(bet.TeamA, agreedAmount);
            var teamBResponse = MapFondeoEquipoResponse(bet.TeamB, agreedAmount);

            return ApplicationResult<FondeoEstadoResponse>.Success(new FondeoEstadoResponse(bet.Id, bet.Status, agreedAmount, teamAResponse, teamBResponse));
        }
        catch (DomainException ex)
        {
            return ApplicationResult<FondeoEstadoResponse>.Failure("DOMAIN_ERROR", ex.Message);
        }
        catch (Exception ex)
        {
            return ApplicationResult<FondeoEstadoResponse>.Failure("ERROR", "Error interno del servidor.");
        }
    }

    private static FondeoEquipoResponse MapFondeoEquipoResponse(BetParty? party, decimal montoAcordado)
    {
        if (party is null)
            return new FondeoEquipoResponse(Guid.Empty, FundingMode.Individual, 0, montoAcordado, 0, false);

        var montoRestante = Math.Max(0, montoAcordado - party.MontoAcumulado);
        var porcentaje = montoAcordado > 0 ? (party.MontoAcumulado / montoAcordado) : 0;

        return new FondeoEquipoResponse(party.Id, party.FundingMode, party.MontoAcumulado, montoRestante, porcentaje, party.EstaCompleto);
    }
}

