using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Silk.Core.Data.Entities;

namespace Silk.Core.Data.EntityConfigurations;

public class GuildLoggingConfigConfiguration : IEntityTypeConfiguration<GuildLoggingConfigEntity>
{

    public void Configure(EntityTypeBuilder<GuildLoggingConfigEntity> builder)
    {
        builder.Property(lc => lc.GuildID)
               .HasConversion(new SnowflakeConverter());

        builder.Property(lc => lc.FallbackChannelID)
               .HasConversion(new NullableSnowflakeConverter());
    }
}