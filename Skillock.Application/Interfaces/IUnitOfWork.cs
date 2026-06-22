namespace Skillock.Application.Interfaces;

/// <summary>
/// Unidad de trabajo. Agrupa todos los repositorios y expone SaveChanges
/// para confirmar la transacción completa en un solo round-trip a la BD.
///
///  USO CORRECTO EN SERVICIOS:
///   1. El servicio obtiene entidades vía repositorios.
///   2. Llama métodos de dominio sobre las entidades.
///   3. Una sola llamada a SaveChangesAsync() al final persiste TODO.
///   Nunca llamar SaveChanges en medio de una operación de negocio.
///
///  PARA OPERACIONES CRÍTICAS (wallet, fondeo):
///   Usar BeginTransactionAsync() para garantizar atomicidad completa
///   con IsolationLevel.RepeatableRead como mínimo.
/// </summary>
public interface IUnitOfWork : IAsyncDisposable
{
    IBetRepository Bets { get; }
    IWalletRepository Wallets { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Abre una transacción de BD explícita. Usar en operaciones que
    /// tocan múltiples agregados (ej: aporte que modifica Wallet + BetParty + WalletTransaction).
    /// </summary>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
