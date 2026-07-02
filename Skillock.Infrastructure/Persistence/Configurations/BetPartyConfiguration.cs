using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skillock.Domain.Models;

namespace Skillock.Infrastructure.Persistence.Configurations;

public class BetPartyConfiguration : IEntityTypeConfiguration<BetParty>
{
    public void Configure(EntityTypeBuilder<BetParty> entity)
    {
        entity.ToTable("BetParties");
        entity.HasKey(e => e.Id);

        entity.Property(e => e.FundingMode).HasConversion<string>().IsRequired();
        entity.Property(e => e.MontoAcumulado).HasColumnType("numeric(18,2)").IsRequired();
        entity.Property(e => e.TeamSize).IsRequired();
        entity.Property(e => e.ModalidadElegida)
            .HasDefaultValue(false)
            .IsRequired();
        entity.Property(e => e.IsTeamA).IsRequired();
        entity.Property(e => e.LiderAcepto).HasDefaultValue(false).IsRequired();
        entity.Property(e => e.EstaCompleto).HasDefaultValue(false).IsRequired();

        entity.ToTable(t => t.HasCheckConstraint("CK_BetParties_TeamSize", "\"TeamSize\" IN (1,3,5)"));

        entity.HasMany(e => e.Members)
            .WithOne(e => e.BetParty)
            .HasForeignKey(e => e.BetPartyId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasOne(e => e.Bet)
            .WithMany(e => e.BetParties)
            .HasForeignKey(e => e.BetId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasIndex(e => new { e.BetId, e.IsTeamA }).IsUnique();
        entity.HasIndex(e => e.BetId);
    }
}

