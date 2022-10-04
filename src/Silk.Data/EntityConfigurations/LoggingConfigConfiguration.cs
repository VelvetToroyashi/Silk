using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Silk.Data.Entities;

namespace Silk.Data.EntityConfigurations;

public class LoggingConfigConfiguration : IEntityTypeConfiguration<GuildLoggingConfigEntity>
{
    public void Configure(EntityTypeBuilder<GuildLoggingConfigEntity> builder)
    {
        builder.ToTable("guild_logging_configs"); 

        builder.Property(p => p.GuildID)
                  .HasColumnName("guild_id");

        builder.Property(p => p.LogMessageEdits)
               .HasColumnName("log_message_edits");

        builder.Property(p => p.LogMessageDeletes)
               .HasColumnName("log_message_deletes");

        builder.Property(p => p.LogInfractions)
               .HasColumnName("log_infractions");

        builder.Property(p => p.LogMemberJoins)
               .HasColumnName("log_member_joins");

        builder.Property(p => p.LogMemberLeaves)
               .HasColumnName("log_member_leaves");

        builder.Property(p => p.UseMobileFriendlyLogging)
               .HasColumnName("use_mobile_friendly_logging");

        builder.Property(p => p.UseWebhookLogging)
               .HasColumnName("use_webhook_logging");

        builder
           .Property(p => p.UseMobileFriendlyLogging)
           .ValueGeneratedNever()
           .HasDefaultValue(true);
    }
}