using GamerBet.Domain.Common;
using GamerBet.Domain.Enums;

namespace GamerBet.Domain.Entities;

/// <summary>
/// Representa un equipo (bando) dentro de una apuesta.
/// Cada Bet tiene exactamente dos BetParty: TeamA y TeamB.
///
/// MontoAcumulado es calculado por la suma de contribuciones activas
/// de sus PartyMembers. Se mantiene como campo desnormalizado para
/// eficiencia en consultas de estado de fondeo, pero SIEMPRE debe
/// verificarse contra la suma real de contribuciones en operaciones críticas.
///
/// 💡 RECOMENDACIÓN: Considera agregar un índice compuesto en (BetId, IsTeamA)
/// en la configuración de Fluent API para acelerar las consultas de fondeo.
/// </summary>
public class BetParty : BaseEntity
{
    public Guid BetId { get; init; }

    /// <summary>True = TeamA, False = TeamB. Simple y eficiente.</summary>
    public required bool IsTeamA { get; init; }

    public FundingMode FundingMode { get; set; } = FundingMode.Individual;

    /// <summary>
    /// Suma de los aportes confirmados de todos los miembros.
    /// NUNCA debe exceder Bet.AgreedAmountPerTeam.
    /// </summary>
    public decimal MontoAcumulado { get; set; }

    /// <summary>True cuando MontoAcumulado == Bet.AgreedAmountPerTeam.</summary>
    public bool EstaCompleto { get; set; }

    /// <summary>True cuando el Líder aceptó formalmente el monto acordado para este equipo.</summary>
    public bool LiderAcepto { get; set; }

    // --- Navegación ---
    public Bet Bet { get; set; } = null!;
    public ICollection<PartyMember> Members { get; set; } = [];

    // --- Métodos de dominio ---

    /// <summary>
    /// Calcula cuánto falta para completar el monto del equipo.
    /// </summary>
    public decimal MontoRestante(decimal montoAcordado) =>
        Math.Max(0, montoAcordado - MontoAcumulado);

    /// <summary>
    /// Valida que un nuevo aporte no exceda el monto acordado.
    /// Regla de negocio: sin excedentes, el último aporte debe ser exactamente el restante.
    /// </summary>
    public bool AporteEsValido(decimal aporte, decimal montoAcordado) =>
        aporte > 0 && (MontoAcumulado + aporte) <= montoAcordado;
}
