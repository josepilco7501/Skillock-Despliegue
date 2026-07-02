using Skillock.Domain.Models;
using Skillock.Domain.Interfaces;
using System.Data;

namespace Skillock.Application.Interfaces;

public interface IUnitOfWork : IAsyncDisposable
{
    IRepository<GameAccount> GameAccounts { get; }
        IUserRepository Users { get; }
    IBetRepository Bets { get; }
    IWalletRepository Wallets { get; }
    IRepository<BetParty> BetParties { get; }
    IRepository<WalletTransaction> WalletTransactions { get; }
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted, CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}

