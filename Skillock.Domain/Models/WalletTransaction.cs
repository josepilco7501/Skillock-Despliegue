using Skillock.Domain.Common;
using Skillock.Domain.Enums;

namespace Skillock.Domain.Models;

/// <summary>
/// Registro inmutable de cada movimiento de fondos en la plataforma.
/// Funciona como ledger contable: NUNCA se actualiza ni elimina un registro existente.
/// 
/// RECOMENDACIÓN: Para auditoría financiera, considera implementar
/// el patrón Event Sourcing en el futuro, donde este ledger sea la
/// fuente de verdad y los balances de Wallet sean proyecciones.
/// </summary>
public class WalletTransaction : BaseEntity
{
    public Guid WalletId { get; set; }
    public required TransactionType Type { get; set; }
    public required decimal Amount { get; set; }

    /// <summary>Saldo disponible DESPUÉS de esta transacción (snapshot para auditoría).</summary>
    public required decimal BalanceAfter { get; set; }

    /// <summary>Referencia a la apuesta relacionada, si aplica.</summary>
    public Guid? BetId { get; set; }

    public string? Description { get; set; }

    // --- Navegación ---
    public virtual Wallet Wallet { get; set; } = null!;
    public virtual Bet? Bet { get; set; }
}

