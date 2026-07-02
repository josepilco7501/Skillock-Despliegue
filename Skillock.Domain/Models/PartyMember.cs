using Skillock.Domain.Common;
using Skillock.Domain.Enums;

namespace Skillock.Domain.Models;

/// <summary>
/// Representa la participación de un usuario en un equipo de una apuesta.
/// Registra su rol (Líder/Miembro) y su contribución económica exacta.
///
/// Esta entidad es el núcleo de la distribución proporcional del premio:
/// al liquidar, el servicio calcula (MontoAportado / AgreedAmountPerTeam)
/// para determinar el porcentaje del premio que corresponde a cada miembro.
///
/// RECOMENDACIÓN: En modo Fondo Individual, solo habrá un PartyMember
/// con Role = Leader y MontoAportado = AgreedAmountPerTeam al 100%.
/// En modo Fondo Mutuo, puede haber N miembros con aportes variables,
/// pero la SUMA debe ser exactamente igual a AgreedAmountPerTeam.
/// </summary>
public class PartyMember(Guid betPartyId, Guid userId) : BaseEntity
{
    public PartyMember() : this(Guid.Empty, Guid.Empty) { }

    public Guid BetPartyId { get; set; } = betPartyId;
    public Guid UserId { get; set; } = userId;
    public required PartyRole Role { get; set; }

    /// <summary>
    /// Monto que este usuario aportó a su equipo.
    /// En Fondo Individual: igual a AgreedAmountPerTeam.
    /// En Fondo Mutuo: cualquier valor positivo hasta completar el total.
    /// </summary>
    public decimal MontoAportado { get; set; }

    /// <summary>
    /// True cuando el aporte fue procesado y los fondos están en SaldoRetenido.
    /// False si el usuario fue invitado pero aún no contribuyó (Fondo Mutuo).
    /// </summary>
    public bool AporteConfirmado { get; set; }

    public DateTime? FechaAporte { get; set; }

    // --- Navegación ---
    public virtual BetParty BetParty { get; set; } = null!;
    public virtual User User { get; set; } = null!;

    // --- Métodos de Dominio ---

    /// <summary>
    /// Calcula la proporción del premio que le corresponde a este miembro.
    /// </summary>
    public decimal CalcularProporcionPremio(decimal premioNeto, decimal montoTotalEquipo)
    {
        if (montoTotalEquipo == 0)
            throw new InvalidOperationException("El monto total del equipo no puede ser cero para calcular la proporción.");

        var proporcion = MontoAportado / montoTotalEquipo;
        return Math.Round(proporcion * premioNeto, 2, MidpointRounding.ToZero);
        // ToZero previene que la suma de proporciones exceda el premioNeto por redondeo.
    }

    /// <summary>
    /// Valida las reglas de negocio para un aporte antes de confirmarlo.
    /// </summary>
    public void ValidarAporte(decimal monto, decimal montoRestante, decimal agreedAmount)
    {
        if (monto <= 0)
            throw new DomainException("El monto debe ser mayor a cero.");

        if (monto > 1000m)
            throw new DomainException("Monto excede límite de S/.1000");

        if (monto > montoRestante)
            throw new ExcesoDeAporteException(monto, montoRestante);

        // Si es Individual, el aporte debe ser exactamente el monto acordado
        if (BetParty != null && BetParty.FundingMode == FundingMode.Individual)
        {
            if (monto != agreedAmount)
                throw new DomainException("En FundingMode Individual el aporte debe ser exactamente el monto acordado.");
        }
    }
}

