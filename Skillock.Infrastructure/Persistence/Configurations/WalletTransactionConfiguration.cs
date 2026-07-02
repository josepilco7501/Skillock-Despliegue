using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skillock.Domain.Models;

namespace Skillock.Infrastructure.Persistence.Configurations;

public class WalletTransactionConfiguration : IEntityTypeConfiguration<WalletTransaction>
{
    public void Configure(EntityTypeBuilder<WalletTransaction> entity)
    {
        entity.ToTable("WalletTransactions");
        entity.HasKey(e => e.Id);

        entity.Property(e => e.Type).HasConversion<string>().IsRequired();
        entity.Property(e => e.Amount).HasColumnType("numeric(18,2)").IsRequired();
        entity.Property(e => e.BalanceAfter).HasColumnType("numeric(18,2)").IsRequired();
        entity.Property(e => e.BetId);
        entity.Property(e => e.Description).HasMaxLength(250);

        entity.HasOne(e => e.Wallet)
            .WithMany(e => e.Transactions)
            .HasForeignKey(e => e.WalletId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(e => e.Bet)
            .WithMany(e => e.Transactions)
            .HasForeignKey(e => e.BetId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.ToTable(t => t.HasTrigger("TR_WalletTransactions_ReadOnly"));
    }
}

