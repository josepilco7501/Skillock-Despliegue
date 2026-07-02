using Skillock.Domain.Common;
using Skillock.Domain.DTOs.Responses;
using Skillock.Domain.Enums;
using Skillock.Domain.Models;

namespace Skillock.Application.Common;

/// <summary>
/// Mapper centralizado para Bet → DTOs de respuesta.
/// Elimina la duplicación de MapBetToResponse en cada Handler.
/// </summary>
public static class BetMapper
{
    public static BetResponse ToResponse(Bet bet)
    {
        var teamAResponse = bet.TeamA is not null ? ToPartyResponse(bet.TeamA) : EmptyTeam(true);
        var teamBResponse = bet.TeamB is not null ? ToPartyResponse(bet.TeamB) : EmptyTeam(false);

        return new BetResponse(
            bet.Id,
            bet.Game,
            bet.Status,
            bet.AgreedAmountPerTeam,
            bet.PremioTotal,
            bet.PremioNeto,
            DomainConstants.PlatformFeePercent,
            bet.MatchId,
            bet.MatchResult,
            bet.ExpiresAt,
            bet.ActivatedAt,
            bet.CompletedAt,
            bet.CreatedAt,
            teamAResponse,
            teamBResponse);
    }

    public static BetPartyResponse ToPartyResponse(BetParty party)
    {
        var members = party.Members
            .Select(m => new PartyMemberResponse(
                m.UserId,
                m.User?.Username ?? "Unknown",   // null-safe
                m.Role,
                m.MontoAportado,
                m.AporteConfirmado,
                m.FechaAporte))
            .ToList()
            .AsReadOnly();

        // decimal? - decimal = decimal? en C#, null-safe sin necesidad de HasValue
        decimal? montoRestante = party.Bet?.AgreedAmountPerTeam - party.MontoAcumulado;

        return new BetPartyResponse(
            party.Id,
            party.IsTeamA,
            party.TeamSize,
            party.FundingMode,
            party.MontoAcumulado,
            montoRestante,
            party.EstaCompleto,
            party.LiderAcepto,
            members);
    }

    private static BetPartyResponse EmptyTeam(bool isTeamA) =>
        new(Guid.Empty, isTeamA, 0, FundingMode.Individual, 0, null, false, false,
            Array.Empty<PartyMemberResponse>());
}
