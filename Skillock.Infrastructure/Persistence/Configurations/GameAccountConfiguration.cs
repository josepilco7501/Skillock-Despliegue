using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skillock.Domain.Models;

namespace Skillock.Infrastructure.Persistence.Configurations;

public class GameAccountConfiguration : IEntityTypeConfiguration<GameAccount>
{
    public void Configure(EntityTypeBuilder<GameAccount> entity)
    {
        entity.ToTable("GameAccounts");
        entity.HasKey(e => e.Id);

        entity.Property(e => e.Game).HasConversion<string>().IsRequired();
        entity.Property(e => e.GamePlayerId).HasMaxLength(100).IsRequired();
        entity.Property(e => e.GamePlayerTag).HasMaxLength(100).IsRequired();
        entity.Property(e => e.IsActive).IsRequired();
        entity.Property(e => e.VerificadoEn);
        entity.Property(e => e.UserId).IsRequired();

        entity.HasOne(e => e.User)
            .WithMany(e => e.GameAccounts)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasIndex(e => new { e.UserId, e.Game }).IsUnique();
    }
}

