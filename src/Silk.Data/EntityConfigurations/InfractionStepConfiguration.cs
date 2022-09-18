using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Silk.Data.Entities;

namespace Silk.Data.EntityConfigurations;

public class InfractionStepConfiguration : IEntityTypeConfiguration<InfractionStepEntity>
{
    public void Configure(EntityTypeBuilder<InfractionStepEntity> builder)
    {
           builder.Property(ifs => ifs.Duration)
                  .HasConversion(d => d.Ticks,
                                 d => TimeSpan.FromTicks(d));
        
        builder.ToTable("infraction_steps");
        
        builder.Property(p => p.ID)
               .HasColumnName("id")
               .IsRequired();
        
        builder.Property(p => p.ConfigId)
               .HasColumnName("config_id")
               .IsRequired();

        builder.Property(p => p.Infractions)
               .HasColumnName("infraction_count")
               .IsRequired();
        
        builder.Property(p => p.Type)
               .HasColumnName("infraction_type")
               .IsRequired();

        builder.Property(p => p.Duration)
               .HasColumnName("infration_duration")
               .IsRequired();
    }
}