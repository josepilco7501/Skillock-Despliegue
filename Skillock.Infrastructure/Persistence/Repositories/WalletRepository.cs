using Microsoft.EntityFrameworkCore;
using Skillock.Application.Interfaces;
using Skillock.Domain.Interfaces;
using Skillock.Domain.Models;
using Skillock.Infrastructure.Context;

namespace Skillock.Infrastructure.Persistence.Repositories;

public class WalletRepository(SkillockDbContext context) : Repository<Wallet>(context), IWalletRepository
{
    public async Task<Wallet?> GetByUserIdWithLockAsync(Guid userId, CancellationToken cancellationToken = default)
        => await Context.Wallets
            .FromSqlRaw("SELECT * FROM \"Wallets\" WHERE \"UserId\" = {0} FOR UPDATE", userId)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<Wallet?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        => await Context.Wallets.FirstOrDefaultAsync(w => w.UserId == userId, cancellationToken);

    public async Task<IReadOnlyList<WalletTransaction>> GetTransaccionesAsync(Guid walletId, int pagina, int tamano, CancellationToken cancellationToken = default)
        => await Context.WalletTransactions
            .AsNoTracking()
            .Where(wt => wt.WalletId == walletId)
            .OrderByDescending(wt => wt.CreatedAt)
            .Skip((Math.Max(pagina, 1) - 1) * Math.Max(tamano, 1))
            .Take(Math.Max(tamano, 1))
            .ToListAsync(cancellationToken);
}

