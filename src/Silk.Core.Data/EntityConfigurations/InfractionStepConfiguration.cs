using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NpgsqlTypes;
using Silk.Core.Data.Entities;

namespace Silk.Core.Data.EntityConfigurations
{
    public class InfractionStepConfiguration : IEntityTypeConfiguration<InfractionStepEntity>
    {
        public void Configure(EntityTypeBuilder<InfractionStepEntity> builder)
        {
            builder.Property(infs => infs.Duration)
                   .HasConversion(d => d.Ticks, d => NpgsqlTimeSpan.FromTicks(d));
        }
    }
}