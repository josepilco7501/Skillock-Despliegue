using Skillock.Domain.Common;
using Skillock.Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;
namespace Skillock.Domain.Models;

/// <summary>
/// Agregado raíz central del dominio. Representa una apuesta PvP completa.
/// 
/// DISEÑO IMPORTANTE:
/// Bet contiene DOS parties (TeamA y TeamB). Cada Party tiene su propia
/// FundingMode, su propio monto acumulado y sus propios contribuyentes.
/// El monto acordado (AgreedAmountPerTeam) es ÚNICO y compartido:
/// ambos equipos deben reunir exactamente ese monto.
///
/// RECOMENDACIÓN: Bet es un Aggregate Root en términos DDD.
/// Considera que PartyMember y BetParty no deberían modificarse
/// directamente desde fuera; encapsula las mutaciones con métodos de dominio aquí.
/// </summary>
public class Bet : BaseEntity
{
    public Bet(EsportGame game, decimal platformFeePercent, Guid creatorGameAccountId)
    {
        Game = game;
        PlatformFeePercent = platformFeePercent;
        CreatorGameAccountId = creatorGameAccountId;
        ExpiresAt = CreatedAt.Add(DomainConstants.TiempoExpiracionApuesta); // ✅ instancia ya existe aquí
    }

    public Bet() : this(EsportGame.Dota2, 0m, Guid.Empty) { }

    public EsportGame Game { get; init; }
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
    public decimal PlatformFeePercent { get; init; }

    public DateTime? ActivatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    // Ventana conjunta de negociación y fondeo
    public DateTime ExpiresAt { get; init; }

    // GameAccount usado por el creador al iniciar la apuesta
    public Guid CreatorGameAccountId { get; init; }

    // Fecha de inicio de la partida (set por infraestructura)
    public DateTime? MatchStartedAt { get; set; }

    // --- Navegación ---
    public virtual ICollection<BetParty> BetParties { get; set; } = new List<BetParty>();
    public virtual ICollection<WalletTransaction> Transactions { get; set; } = new List<WalletTransaction>();

    // --- Propiedades Computed ---

    /// <summary>Premio bruto total = suma de aportes de ambos equipos.</summary>
    public decimal? PremioTotal =>
        BetParties?.Any() == true ? BetParties.Sum(bp => bp.MontoAcumulado) : (decimal?)null;

    /// <summary>Premio neto después de descontar la comisión de plataforma.</summary>
    public decimal? PremioNeto => PremioTotal.HasValue
        ? Math.Round(PremioTotal.Value * (1 - PlatformFeePercent), 2, MidpointRounding.ToZero)
        : null;

    /// <summary>Team A (IsTeamA = true). Propiedad computed para acceso conveniente.</summary>
    [NotMapped]
    public BetParty? TeamA => BetParties.FirstOrDefault(bp => bp.IsTeamA);

    /// <summary>Team B (IsTeamA = false). Propiedad computed para acceso conveniente.</summary>
    [NotMapped]
    public BetParty? TeamB => BetParties.FirstOrDefault(bp => !bp.IsTeamA);

    // --- Métodos de Dominio ---

    /// <summary>
    /// Verifica si ambos equipos completaron exactamente el monto acordado.
    /// Usado por el servicio de Application para transicionar a Active.
    /// </summary>
    public bool AmbosEquiposCompletos()
        => AgreedAmountPerTeam.HasValue
           && TeamA?.MontoAcumulado == AgreedAmountPerTeam.Value
           && TeamB?.MontoAcumulado == AgreedAmountPerTeam.Value;

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
    public bool EsTransicionValida(BetStatus nuevoEstado)
    {
        // Si la apuesta expiró y no está Active/Completed, la única transición permitida es a Cancelled
        if (DateTime.UtcNow > ExpiresAt && Status != BetStatus.Active && Status != BetStatus.Completed)
            return nuevoEstado == BetStatus.Cancelled;

        return (Status, nuevoEstado) switch
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

    public static void ValidarCombinacionEquipos(int sizeA, int sizeB)
    {
        var ok = (sizeA, sizeB) switch
        {
            (1, 1) => true,
            (1, 3) => true,
            (3, 1) => true,
            (1, 5) => true,
            (5, 1) => true,
            (3, 3) => true,
            (5, 5) => true,
            _ => false
        };

        if (!ok) throw new CombinacionEquiposInvalidaException(sizeA, sizeB);
    }
}

