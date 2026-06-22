using Skillock.Domain.Enums;

namespace Skillock.Application.DTOs.Requests;

/// <summary>
/// El Líder crea la apuesta inicial. Solo necesita definir el juego
/// y opcionalmente un monto inicial propuesto (puede negociarse después).
/// </summary>
public record CrearApuestaRequest(
    EsportGame Game,
    decimal? MontoInicialPropuesto,
    decimal PlatformFeePercent = 0.05m
);

/// <summary>
/// Propuesta o contra-propuesta de monto durante la negociación.
/// El monto debe ser positivo. Quien propone confirma implícitamente su propio lado.
/// </summary>
public record ProponerMontoRequest(
    decimal MontoPerTeam
);

/// <summary>
/// El líder elige la modalidad de fondeo de su equipo.
/// Individual = él cubre el 100%. Mutual = varios miembros aportan.
/// </summary>
public record ElegirModalidadRequest(
    FundingMode Modalidad
);

/// <summary>
/// Aporte de fondos de un miembro a su equipo.
///
/// 💡 En Fondo Individual: Monto debe ser exactamente AgreedAmountPerTeam.
///    En Fondo Mutuo: Monto puede ser cualquier valor positivo
///    siempre que no exceda BetParty.MontoRestante().
/// </summary>
public record AporteRequest(
    Guid BetPartyId,
    decimal Monto
);

/// <summary>
/// Invitación de un líder a un usuario para unirse a su equipo (Fondo Mutuo).
/// </summary>
public record InvitarMiembroRequest(
    Guid BetPartyId,
    Guid UsuarioInvitadoId
);

/// <summary>
/// Parámetros de paginación reutilizables en cualquier endpoint de historial.
/// </summary>
public record PaginacionRequest(
    int Pagina = 1,
    int Tamano = 20
)
{
    /// <summary>
    /// Tamaño máximo permitido por página para evitar queries masivas.
    /// </summary>
    public int TamanoSeguro => Math.Min(Tamano, 100);
};
