using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skillock.Domain.Models;

namespace Skillock.Infrastructure.Persistence.Configurations;

public class BetConfiguration : IEntityTypeConfiguration<Bet>
{
    public void Configure(EntityTypeBuilder<Bet> entity)
    {
        entity.ToTable("Bets");
        entity.HasKey(e => e.Id);

        entity.Property(e => e.Game).HasConversion<string>().IsRequired();
        entity.Property(e => e.Status).HasConversion<string>().IsRequired();
        entity.Property(e => e.MatchResult).HasConversion<string>().IsRequired();
        entity.Property(e => e.PlatformFeePercent).HasColumnType("numeric(5,4)").IsRequired();
        entity.Property(e => e.AgreedAmountPerTeam).HasColumnType("numeric(18,2)");
        entity.Property(e => e.ExpiresAt).IsRequired();
        entity.Property(e => e.CreatorGameAccountId).IsRequired();
        entity.Property(e => e.MatchId).HasMaxLength(100);
        entity.Property(e => e.MatchStartedAt);
        entity.Property(e => e.ActivatedAt);
        entity.Property(e => e.CompletedAt);

        entity.HasMany(e => e.BetParties)
            .WithOne(e => e.Bet)
            .HasForeignKey(e => e.BetId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasMany(e => e.Transactions)
            .WithOne(e => e.Bet)
            .HasForeignKey(e => e.BetId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasIndex(e => e.ExpiresAt);
        entity.HasIndex(e => e.Status);
    }
}

