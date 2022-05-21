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
    }
}