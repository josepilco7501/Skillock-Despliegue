using GamerBet.Domain.Common;
using GamerBet.Domain.Enums;

namespace GamerBet.Domain.Entities;

/// <summary>
/// Registro inmutable de cada movimiento de fondos en la plataforma.
/// Funciona como ledger contable: NUNCA se actualiza ni elimina un registro existente.
/// 
/// 💡 RECOMENDACIÓN: Para auditoría financiera, considera implementar
/// el patrón Event Sourcing en el futuro, donde este ledger sea la
/// fuente de verdad y los balances de Wallet sean proyecciones.
/// </summary>
public class WalletTransaction : BaseEntity
{
    public Guid WalletId { get; init; }
    public required TransactionType Type { get; init; }
    public required decimal Amount { get; init; }

    /// <summary>Saldo disponible DESPUÉS de esta transacción (snapshot para auditoría).</summary>
    public required decimal BalanceAfter { get; init; }

    /// <summary>Referencia a la apuesta relacionada, si aplica.</summary>
    public Guid? BetId { get; init; }

    public string? Description { get; init; }

    // --- Navegación ---
    public Wallet Wallet { get; set; } = null!;
    public Bet? Bet { get; set; }
}
