using GamerBet.Domain.Common;
using GamerBet.Domain.Enums;

namespace GamerBet.Domain.Entities;

/// <summary>
/// Agregado raíz central del dominio. Representa una apuesta PvP completa.
/// 
/// DISEÑO IMPORTANTE:
/// Bet contiene DOS parties (TeamA y TeamB). Cada Party tiene su propia
/// FundingMode, su propio monto acumulado y sus propios contribuyentes.
/// El monto acordado (AgreedAmountPerTeam) es ÚNICO y compartido:
/// ambos equipos deben reunir exactamente ese monto.
///
/// 💡 RECOMENDACIÓN: Bet es un Aggregate Root en términos DDD.
/// Considera que PartyMember y BetParty no deberían modificarse
/// directamente desde fuera; encapsula las mutaciones con métodos de dominio aquí.
/// </summary>
public class Bet : BaseEntity
{
    public required EsportGame Game { get; init; }
    public BetStatus Status { get; set; } = BetStatus.Draft;

    /// <summary>
    /// Monto fijo que CADA equipo debe completar. Se establece durante
    /// la negociación y es inmutable una vez en estado Agreed o posterior.
    /// </summary>
    public decimal? AgreedAmountPerTeam { get; set; }

    /// <summary>
    /// ID de la partida en la API del juego (Dota2/CS2/Valorant).
    /// Se vincula al activar la apuesta (estado Active).
    /// </summary>
    public string? MatchId { get; set; }

    public MatchResult MatchResult { get; set; } = MatchResult.Pending;

    /// <summary>Porcentaje de comisión de plataforma (ej: 0.05m = 5%).</summary>
    public required decimal PlatformFeePercent { get; init; }

    /// <summary>Premio bruto total = AgreedAmountPerTeam * 2.</summary>
    public decimal? PremioTotal => AgreedAmountPerTeam.HasValue
        ? AgreedAmountPerTeam.Value * 2
        : null;

    /// <summary>Premio neto después de descontar la comisión de plataforma.</summary>
    public decimal? PremioNeto => PremioTotal.HasValue
        ? PremioTotal.Value * (1 - PlatformFeePercent)
        : null;

    public DateTime? ActivatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    // --- Navegación ---
    public BetParty TeamA { get; set; } = null!;
    public BetParty TeamB { get; set; } = null!;
    public ICollection<WalletTransaction> Transactions { get; set; } = [];

    // --- Métodos de dominio ---

    /// <summary>
    /// Verifica si ambos equipos completaron exactamente el monto acordado.
    /// Usado por el servicio de Application para transicionar a Active.
    /// </summary>
    public bool AmbosEquiposCompletos()
        => AgreedAmountPerTeam.HasValue
           && TeamA.MontoAcumulado == AgreedAmountPerTeam.Value
           && TeamB.MontoAcumulado == AgreedAmountPerTeam.Value;

    /// <summary>
    /// Retorna la party ganadora según el resultado de la partida.
    /// </summary>
    public BetParty? ObtenerPartyGanadora() => MatchResult switch
    {
        MatchResult.TeamAWins => TeamA,
        MatchResult.TeamBWins => TeamB,
        _ => null
    };

    /// <summary>
    /// Retorna la party perdedora según el resultado de la partida.
    /// </summary>
    public BetParty? ObtenerPartyPerdedora() => MatchResult switch
    {
        MatchResult.TeamAWins => TeamB,
        MatchResult.TeamBWins => TeamA,
        _ => null
    };

    /// <summary>
    /// Valida que una transición de estado sea legal según la máquina de estados.
    /// </summary>
    public bool EsTransicionValida(BetStatus nuevoEstado) => (Status, nuevoEstado) switch
    {
        (BetStatus.Draft, BetStatus.Negotiating) => true,
        (BetStatus.Negotiating, BetStatus.Agreed) => true,
        (BetStatus.Agreed, BetStatus.Funding) => true,
        (BetStatus.Funding, BetStatus.Active) => true,
        (BetStatus.Active, BetStatus.Completed) => true,
        (BetStatus.Draft, BetStatus.Cancelled) => true,
        (BetStatus.Negotiating, BetStatus.Cancelled) => true,
        (BetStatus.Agreed, BetStatus.Cancelled) => true,
        (BetStatus.Funding, BetStatus.Cancelled) => true,
        (BetStatus.Active, BetStatus.Disputed) => true,
        _ => false
    };
}
