using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skillock.Domain.Models;

namespace Skillock.Infrastructure.Persistence.Configurations;

public class WalletConfiguration : IEntityTypeConfiguration<Wallet>
{
    public void Configure(EntityTypeBuilder<Wallet> entity)
    {
        entity.ToTable("Wallets");
        entity.HasKey(e => e.Id);

        entity.Property(e => e.SaldoDisponible).HasColumnType("numeric(18,2)").IsRequired();
        entity.Property(e => e.SaldoRetenido).HasColumnType("numeric(18,2)").IsRequired();
        entity.Property(e => e.UserId).IsRequired();

        entity.HasOne(e => e.User)
            .WithOne(e => e.Wallet)
            .HasForeignKey<Wallet>(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasMany(e => e.Transactions)
            .WithOne(e => e.Wallet)
            .HasForeignKey(e => e.WalletId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasIndex(e => e.UserId).IsUnique();
    }
}

