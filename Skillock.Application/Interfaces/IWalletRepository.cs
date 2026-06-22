using Skillock.Domain.Entities;

namespace Skillock.Application.Interfaces;

/// <summary>
/// Repositorio especializado para Wallet y WalletTransaction.
/// </summary>
public interface IWalletRepository : IRepository<Wallet>
{
    /// <summary>
    /// Obtiene la wallet de un usuario con bloqueo pesimista de fila (FOR UPDATE).
    /// OBLIGATORIO usar este método en cualquier operación que modifique saldos,
    /// combinado con BeginTransactionAsync en IUnitOfWork.
    ///
    ///  El bloqueo pesimista es la segunda línea de defensa contra double-spending.
    ///    La primera es la transacción serializable. Juntas eliminan race conditions.
    /// </summary>
    Task<Wallet?> GetByUserIdWithLockAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<Wallet?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Historial de transacciones paginado para el dashboard del usuario.
    /// </summary>
    Task<IReadOnlyList<WalletTransaction>> GetTransaccionesAsync(
        Guid walletId,
        int pagina,
        int tamano,
        CancellationToken cancellationToken = default);
}
