using Microsoft.EntityFrameworkCore;
using Skillock.Domain.Common;
using Skillock.Domain.Models;
using Skillock.Infrastructure.Persistence.Configurations;

namespace Skillock.Infrastructure.Context;

public class SkillockDbContext(DbContextOptions<SkillockDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Bet> Bets => Set<Bet>();
    public DbSet<BetParty> BetParties => Set<BetParty>();
    public DbSet<PartyMember> PartyMembers => Set<PartyMember>();
    public DbSet<Wallet> Wallets => Set<Wallet>();
    public DbSet<WalletTransaction> WalletTransactions => Set<WalletTransaction>();
    public DbSet<GameAccount> GameAccounts => Set<GameAccount>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new GameAccountConfiguration());
        modelBuilder.ApplyConfiguration(new WalletConfiguration());
        modelBuilder.ApplyConfiguration(new BetConfiguration());
        modelBuilder.ApplyConfiguration(new BetPartyConfiguration());
        modelBuilder.ApplyConfiguration(new PartyMemberConfiguration());
        modelBuilder.ApplyConfiguration(new WalletTransactionConfiguration());
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries<BaseEntity>()
            .Where(e => e.State == EntityState.Modified);

        foreach (var entry in entries)
            entry.Entity.UpdatedAt = DateTime.UtcNow;

        return base.SaveChangesAsync(cancellationToken);
    }
}

