using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Silk.Data.Entities;

namespace Silk.Data.EntityConfigurations;

public class ExemptionConfiguration : IEntityTypeConfiguration<ExemptionEntity>
{
    public void Configure(EntityTypeBuilder<ExemptionEntity> builder)
    {
        builder.ToTable("infraction_exemptions");
        
        builder.HasKey(e => e.ID);
        
        builder.Property(e => e.ID)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();
        
        builder.Property(p => p.Exemption)
            .HasColumnName("exemption_type")
            .IsRequired();
        
        builder.Property(p => p.TargetType)
               .HasColumnName("type")
               .IsRequired();
        
        builder.Property(p => p.TargetID)
               .HasColumnName("target_id")
               .IsRequired();
        
        builder.Property(p => p.GuildID)
               .HasColumnName("guild_id")
               .IsRequired();
    }
}