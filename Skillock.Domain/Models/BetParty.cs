using Skillock.Domain.Common;
using Skillock.Domain.Enums;
using Skillock.Domain.Models;

namespace Skillock.Domain.Models;

/// <summary>
/// Representa un equipo (bando) dentro de una apuesta.
/// Cada Bet tiene exactamente dos BetParty: TeamA y TeamB.
///
/// MontoAcumulado es calculado por la suma de contribuciones activas
/// de sus PartyMembers. Se mantiene como campo desnormalizado para
/// eficiencia en consultas de estado de fondeo, pero SIEMPRE debe
/// verificarse contra la suma real de contribuciones en operaciones críticas.
///
/// RECOMENDACIÓN: Considera agregar un índice compuesto en (BetId, IsTeamA)
/// en la configuración de Fluent API para acelerar las consultas de fondeo.
/// </summary>
public class BetParty(int teamSize) : BaseEntity
{
    public BetParty() : this(1) { }

    public Guid BetId { get; set; }

    /// <summary>True = TeamA, False = TeamB. Simple y eficiente.</summary>
    public required bool IsTeamA { get; set; }
    
    public bool ModalidadElegida { get; set; } = false;

    public FundingMode FundingMode { get; set; } = FundingMode.Individual;

    /// <summary>Tamaño fijo del equipo (1,3,5).</summary>
    public int TeamSize { get; set; } = teamSize;

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
    public virtual Bet Bet { get; set; } = null!;
    public virtual ICollection<PartyMember> Members { get; set; } = new List<PartyMember>();

    // --- Métodos de Dominio ---

    public void ValidarTeamSize()
    {
        if (TeamSize != 1 && TeamSize != 3 && TeamSize != 5)
            throw new DomainException("TeamSize inválido. Solo se permiten 1, 3 o 5 jugadores.");
    }

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

    public void ValidarInvariantes(decimal? agreedAmount)
    {
        ValidarTeamSize();

        if (agreedAmount.HasValue && MontoAcumulado > agreedAmount.Value)
            throw new DomainException("MontoAcumulado no puede superar el monto acordado.");

        if (FundingMode == FundingMode.Individual && Members.Count > 1)
            throw new DomainException("FundingMode Individual no permite más de un miembro en el equipo.");
    }
}

