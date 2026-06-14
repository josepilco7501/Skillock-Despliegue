using GamerBet.Domain.Common;
using GamerBet.Domain.Enums;

namespace GamerBet.Domain.Entities;

/// <summary>
/// Representa la participación de un usuario en un equipo de una apuesta.
/// Registra su rol (Líder/Miembro) y su contribución económica exacta.
///
/// Esta entidad es el núcleo de la distribución proporcional del premio:
/// al liquidar, el servicio calcula (MontoAportado / AgreedAmountPerTeam)
/// para determinar el porcentaje del premio que corresponde a cada miembro.
///
/// 💡 RECOMENDACIÓN: En modo Fondo Individual, solo habrá un PartyMember
/// con Role = Leader y MontoAportado = AgreedAmountPerTeam al 100%.
/// En modo Fondo Mutuo, puede haber N miembros con aportes variables,
/// pero la SUMA debe ser exactamente igual a AgreedAmountPerTeam.
/// </summary>
public class PartyMember : BaseEntity
{
    public Guid BetPartyId { get; init; }
    public Guid UserId { get; init; }
    public required PartyRole Role { get; init; }

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
    public BetParty BetParty { get; set; } = null!;
    public User User { get; set; } = null!;

    // --- Métodos de dominio ---

    /// <summary>
    /// Calcula la proporción del premio que le corresponde a este miembro.
    /// </summary>
    public decimal CalcularProporcionPremio(decimal premioNeto, decimal montoTotalEquipo)
    {
        if (montoTotalEquipo == 0)
            throw new InvalidOperationException("El monto total del equipo no puede ser cero para calcular la proporción.");

        var proporcion = MontoAportado / montoTotalEquipo;
        return Math.Round(proporcion * premioNeto, 2, MidpointRounding.ToZero);
        // 💡 ToZero previene que la suma de proporciones exceda el premioNeto por redondeo.
    }
}
