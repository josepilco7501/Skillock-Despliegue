using Skillock.Domain.Enums;

namespace Skillock.Domain.DTOs.Responses;

public record BetResponse(
    Guid Id,
    EsportGame Game,
    BetStatus Status,
    decimal? AgreedAmountPerTeam,
    decimal? PremioTotal,
    decimal? PremioNeto,
    decimal PlatformFeePercent,
    string? MatchId,
    MatchResult MatchResult,
    DateTime ExpiresAt,
    DateTime? ActivatedAt,
    DateTime? CompletedAt,
    DateTime CreatedAt,
    BetPartyResponse TeamA,
    BetPartyResponse TeamB
);

public record BetPartyResponse(
    Guid Id,
    bool IsTeamA,
    int TeamSize,
    FundingMode FundingMode,
    decimal MontoAcumulado,
    decimal? MontoRestante,
    bool EstaCompleto,
    bool LiderAcepto,
    IReadOnlyList<PartyMemberResponse> Members
);

public record PartyMemberResponse(
    Guid UserId,
    string Username,
    PartyRole Role,
    decimal MontoAportado,
    bool AporteConfirmado,
    DateTime? FechaAporte
);

public record BetResumenResponse(
    Guid Id,
    EsportGame Game,
    BetStatus Status,
    decimal? AgreedAmountPerTeam,
    decimal? PremioNeto,
    string? MatchId,
    DateTime CreatedAt,
    DateTime? CompletedAt,
    string? RivalLiderUsername
);

public record AporteResponse(
    Guid BetId,
    Guid PartyId,
    decimal MontoAportado,
    decimal NuevoMontoAcumulado,
    decimal MontoRestante,
    bool EquipoCompleto,
    bool ApuestaActivada,
    decimal NuevoSaldoDisponible
);

public record FondeoEstadoResponse(
    Guid BetId,
    BetStatus StatusApuesta,
    decimal AgreedAmountPerTeam,
    FondeoEquipoResponse TeamA,
    FondeoEquipoResponse TeamB
);

public record FondeoEquipoResponse(
    Guid PartyId,
    FundingMode Modalidad,
    decimal MontoAcumulado,
    decimal MontoRestante,
    decimal PorcentajeCompletado,
    bool EstaCompleto
);

public record LiquidacionResponse(
    Guid BetId,
    MatchResult Resultado,
    decimal PremioTotal,
    decimal PlatformFee,
    decimal PremioNeto,
    IReadOnlyList<PagoIndividualResponse> PagosGanadores,
    DateTime LiquidadoEn
);

public record PagoIndividualResponse(
    Guid UserId,
    string Username,
    decimal MontoAportado,
    decimal PremioRecibido,
    decimal Proporcion
);

public record PagedResponse<T>(
    IReadOnlyList<T> Items,
    int Pagina,
    int Tamano,
    int TotalItems,
    int TotalPaginas
)
{
    public bool TienePaginaAnterior => Pagina > 1;
    public bool TienePaginaSiguiente => Pagina < TotalPaginas;
}

