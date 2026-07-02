using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skillock.Domain.Models;

namespace Skillock.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> entity)
    {
        entity.ToTable("Users");
        entity.HasKey(e => e.Id);

        entity.Property(e => e.Username).HasMaxLength(50).IsRequired();
        entity.Property(e => e.Email).HasMaxLength(100).IsRequired();
        entity.Property(e => e.PasswordHash).IsRequired();
        entity.Property(e => e.AvatarUrl).HasMaxLength(500);
        entity.Property(e => e.IsActive).IsRequired();

        entity.HasIndex(e => e.Email).IsUnique();
        entity.HasIndex(e => e.Username).IsUnique();

        entity.HasMany(e => e.GameAccounts)
            .WithOne(e => e.User)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasOne(e => e.Wallet)
            .WithOne(e => e.User)
            .HasForeignKey<Wallet>(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

