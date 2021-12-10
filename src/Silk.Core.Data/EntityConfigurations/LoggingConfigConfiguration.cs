using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Silk.Core.Data.Entities;

namespace Silk.Core.Data.EntityConfigurations;

public class LoggingConfigConfiguration : IEntityTypeConfiguration<LoggingChannelEntity>
{

    public void Configure(EntityTypeBuilder<LoggingChannelEntity> builder)
    {
        builder.Property(lc => lc.ChannelID)
               .HasConversion<SnowflakeConverter>();
        
        builder.Property(lc => lc.WebhookID)
               .HasConversion<SnowflakeConverter>();
        
        builder.Property(lc => lc.GuildId)
               .HasConversion<SnowflakeConverter>();
    }
}