    using Microsoft.EntityFrameworkCore.Storage;
    using Skillock.Application.Interfaces;
    using Skillock.Domain.Interfaces;
    using Skillock.Domain.Models;
    using Skillock.Infrastructure.Persistence.Repositories;
    using System.Data;
    using Skillock.Infrastructure.Context;

    namespace Skillock.Infrastructure.Persistence;

    public class UnitOfWork(SkillockDbContext context) : IUnitOfWork
    {
        private IBetRepository? _bets;
        private IWalletRepository? _wallets;
        private IUserRepository? _users;
        private IRepository<GameAccount>? _gameAccounts;
        private IRepository<WalletTransaction>? _walletTransactions;
        private IDbContextTransaction? _transaction;
        
        private IRepository<BetParty>? _betParties;
        public IRepository<BetParty> BetParties => _betParties ??= new Repository<BetParty>(context);

        public IBetRepository Bets => _bets ??= new BetRepository(context);
        public IWalletRepository Wallets => _wallets ??= new WalletRepository(context);
        public IUserRepository Users => _users ??= new UserRepository(context);
        public IRepository<GameAccount> GameAccounts => _gameAccounts ??= new Repository<GameAccount>(context);
        public IRepository<WalletTransaction> WalletTransactions => _walletTransactions ??= new Repository<WalletTransaction>(context);

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
            => context.SaveChangesAsync(cancellationToken);

        public Task BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted, CancellationToken cancellationToken = default)
            => BeginTransactionInternalAsync(isolationLevel, cancellationToken);

        private async Task BeginTransactionInternalAsync(IsolationLevel isolationLevel, CancellationToken cancellationToken)
        {
            _transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        }

        public Task CommitTransactionAsync(CancellationToken cancellationToken = default)
            => _transaction?.CommitAsync(cancellationToken) ?? Task.CompletedTask;

        public Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
            => _transaction?.RollbackAsync(cancellationToken) ?? Task.CompletedTask;

        public async ValueTask DisposeAsync()
        {
            if (_transaction is not null)
                await _transaction.DisposeAsync();

            await context.DisposeAsync();
        }
    }




