using GamerBet.Domain.Common;

namespace GamerBet.Domain.Entities;

/// <summary>
/// Wallet del usuario. Separa el saldo en dos baldes lógicos para garantizar
/// que los fondos comprometidos en una apuesta no puedan usarse en otra.
///
/// 💡 RECOMENDACIÓN CRÍTICA DE CONCURRENCIA:
/// Aunque se descartó RowVersion/xmin, es OBLIGATORIO que cualquier operación
/// que modifique SaldoDisponible o SaldoRetenido se ejecute dentro de una
/// transacción EF Core serializable o al menos Repeatable Read para evitar
/// double-spending. Ejemplo en Application:
///   await using var tx = await _context.Database.BeginTransactionAsync();
/// </summary>
public class Wallet : BaseEntity
{
    public Guid UserId { get; init; }

    /// <summary>Fondos libres. Puede apostar o retirar.</summary>
    public decimal SaldoDisponible { get; set; }

    /// <summary>
    /// Fondos congelados mientras una apuesta está en estado Funding o Active.
    /// Solo se liberan al Completar (payout) o Cancelar (refund).
    /// </summary>
    public decimal SaldoRetenido { get; set; }

    /// <summary>Saldo total contable = Disponible + Retenido.</summary>
    public decimal SaldoTotal => SaldoDisponible + SaldoRetenido;

    // --- Navegación ---
    public User User { get; set; } = null!;
    public ICollection<WalletTransaction> Transactions { get; set; } = [];

    // --- Métodos de dominio ---

    /// <summary>
    /// Mueve fondos de Disponible a Retenido al confirmar una contribución.
    /// Lanza excepción de dominio si no hay saldo suficiente.
    /// </summary>
    public void ReservarFondos(decimal monto)
    {
        if (monto <= 0)
            throw new InvalidOperationException("El monto a reservar debe ser mayor a cero.");

        if (SaldoDisponible < monto)
            throw new InvalidOperationException(
                $"Saldo insuficiente. Disponible: {SaldoDisponible:C}, requerido: {monto:C}.");

        SaldoDisponible -= monto;
        SaldoRetenido += monto;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Libera fondos retenidos de vuelta a Disponible (refund por cancelación).
    /// </summary>
    public void LiberarFondos(decimal monto)
    {
        if (monto <= 0)
            throw new InvalidOperationException("El monto a liberar debe ser mayor a cero.");

        if (SaldoRetenido < monto)
            throw new InvalidOperationException(
                $"Saldo retenido insuficiente para liberar. Retenido: {SaldoRetenido:C}, solicitado: {monto:C}.");

        SaldoRetenido -= monto;
        SaldoDisponible += monto;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Liquida el premio: elimina los fondos retenidos (ya se jugaron) y
    /// acredita el payout neto en Disponible.
    /// </summary>
    public void AcreditarPremio(decimal montoRetenidoOriginal, decimal premioNeto)
    {
        if (SaldoRetenido < montoRetenidoOriginal)
            throw new InvalidOperationException("Inconsistencia contable al liquidar premio.");

        SaldoRetenido -= montoRetenidoOriginal;
        SaldoDisponible += premioNeto;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Consume saldo retenido sin acreditar premio (bando perdedor).
    /// </summary>
    public void ConsumirRetenido(decimal monto)
    {
        if (SaldoRetenido < monto)
            throw new InvalidOperationException("Inconsistencia contable al consumir saldo retenido.");

        SaldoRetenido -= monto;
        UpdatedAt = DateTime.UtcNow;
    }
}
