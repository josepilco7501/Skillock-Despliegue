using GamerBet.Domain.Enums;

namespace GamerBet.Application.DTOs.Responses;

/// <summary>
/// Respuesta completa de una apuesta con todas sus parties y miembros.
/// Usado en la vista de detalle y después de cada operación de negocio.
/// </summary>
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
    DateTime? ActivatedAt,
    DateTime? CompletedAt,
    DateTime CreatedAt,
    BetPartyResponse TeamA,
    BetPartyResponse TeamB
);

/// <summary>
/// Estado de un equipo dentro de la apuesta.
/// </summary>
public record BetPartyResponse(
    Guid Id,
    bool IsTeamA,
    FundingMode FundingMode,
    decimal MontoAcumulado,
    decimal? MontoRestante,
    bool EstaCompleto,
    bool LiderAcepto,
    IReadOnlyList<PartyMemberResponse> Members
);

/// <summary>
/// Contribución individual de un miembro.
/// </summary>
public record PartyMemberResponse(
    Guid UserId,
    string Username,
    PartyRole Role,
    decimal MontoAportado,
    bool AporteConfirmado,
    DateTime? FechaAporte
);

/// <summary>
/// Versión resumida para listados e historial. Sin detalle de miembros.
/// </summary>
public record BetResumenResponse(
    Guid Id,
    EsportGame Game,
    BetStatus Status,
    decimal? AgreedAmountPerTeam,
    decimal? PremioNeto,
    DateTime CreatedAt,
    DateTime? CompletedAt,
    /// <summary>Nombre del equipo contrario al usuario consultante.</summary>
    string? RivalLiderUsername
);

/// <summary>
/// Respuesta tras procesar un aporte. Incluye el estado actualizado del fondeo
/// para que el frontend actualice la barra de progreso sin una segunda llamada.
/// </summary>
public record AporteResponse(
    Guid BetId,
    Guid PartyId,
    decimal MontoAportado,
    decimal NuevoMontoAcumulado,
    decimal MontoRestante,
    bool EquipoCompleto,
    bool ApuestaActivada,
    /// <summary>Nuevo saldo disponible del usuario tras el aporte.</summary>
    decimal NuevoSaldoDisponible
);

/// <summary>
/// Estado detallado del fondeo de ambos equipos.
/// Pensado para el endpoint de polling del frontend (barra de progreso en tiempo real).
/// </summary>
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

/// <summary>
/// Resultado de la liquidación. Contiene el detalle de cada pago
/// para registros de auditoría y para notificar a los ganadores.
/// </summary>
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
    decimal PropOrcion
);

/// <summary>
/// Resultado de consulta a la API del juego.
/// </summary>
public record MatchResultResponse(
    string MatchId,
    MatchResult Resultado,
    DateTime FinalizadoEn,
    /// <summary>Datos adicionales crudos de la API para trazabilidad.</summary>
    string? RawApiResponse
);

/// <summary>
/// Wrapper genérico de paginación para cualquier listado.
/// </summary>
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
};
