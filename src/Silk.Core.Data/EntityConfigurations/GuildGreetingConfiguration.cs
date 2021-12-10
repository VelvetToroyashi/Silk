using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Silk.Core.Data.Entities;

namespace Silk.Core.Data.EntityConfigurations;

public class GuildGreetingConfiguration : IEntityTypeConfiguration<GuildGreetingEntity>
{

    public void Configure(EntityTypeBuilder<GuildGreetingEntity> builder)
    {
        builder.Property(gg => gg.ChannelID)
               .HasConversion<SnowflakeConverter>();

        builder.Property(gg => gg.GuildID)
               .HasConversion<SnowflakeConverter>();
        
        builder.Property(gg => gg.MetadataID)
               .HasConversion<SnowflakeConverter>();
    }
}