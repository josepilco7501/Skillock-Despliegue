using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skillock.Domain.Models;

namespace Skillock.Infrastructure.Persistence.Configurations;

public class PartyMemberConfiguration : IEntityTypeConfiguration<PartyMember>
{
    public void Configure(EntityTypeBuilder<PartyMember> entity)
    {
        entity.ToTable("PartyMembers");
        entity.HasKey(e => e.Id);

        entity.Property(e => e.MontoAportado).HasColumnType("numeric(18,2)").IsRequired();
        entity.Property(e => e.Role).HasConversion<string>().IsRequired();
        entity.Property(e => e.AporteConfirmado).IsRequired();
        entity.Property(e => e.FechaAporte);

        entity.HasOne(e => e.User)
            .WithMany(u => u.PartyMemberships)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(e => e.BetParty)
            .WithMany(e => e.Members)
            .HasForeignKey(e => e.BetPartyId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasIndex(e => new { e.BetPartyId, e.UserId }).IsUnique();
        entity.HasIndex(e => e.UserId);
    }
}

