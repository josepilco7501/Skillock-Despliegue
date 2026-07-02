using Skillock.Domain.Interfaces;
using Skillock.Domain.Models;

namespace Skillock.Domain.Interfaces;

public interface IWalletRepository : IRepository<Wallet>
{
    Task<Wallet?> GetByUserIdWithLockAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Wallet?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<WalletTransaction>> GetTransaccionesAsync(Guid walletId, int pagina, int tamano, CancellationToken cancellationToken = default);
}

