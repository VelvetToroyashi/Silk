using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Silk.Data.Entities;

namespace Silk.Data.EntityConfigurations;

public class LoggingChannelConfiguration : IEntityTypeConfiguration<LoggingChannelEntity>
{
    public void Configure(EntityTypeBuilder<LoggingChannelEntity> builder)
    {
        builder.ToTable("logging_channels");

        builder.Property(p => p.GuildID)
               .HasColumnName("guild_id");
        
        builder.Property(p => p.WebhookID)
               .HasColumnName("webhook_id");
        
        builder.Property(p => p.WebhookToken)
               .HasColumnName("webhook_token");
        
        builder.Property(p => p.ChannelID)
               .HasColumnName("channel_id");
        
        
    }
}