using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Silk.Data.Entities;

namespace Silk.Data.EntityConfigurations;

public class ReminderConfiguration : IEntityTypeConfiguration<ReminderEntity>
{

    public void Configure(EntityTypeBuilder<ReminderEntity> builder)
    {
        builder.ToTable("reminders");
        
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Id)
            .HasColumnName("id")
            .IsRequired();
        
        builder.Property(p => p.ExpiresAt)
            .HasColumnName("expires_at")
            .IsRequired();
        
        builder.Property(p => p.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();
        
        builder.Property(p => p.IsPrivate)
            .HasColumnName("is_private")
            .IsRequired();
        
        builder.Property(p => p.IsReply)
            .HasColumnName("is_reply")
            .IsRequired();
        
        builder.Property(p => p.OwnerID)
            .HasColumnName("owner_id")
            .IsRequired();
        
        builder.Property(p => p.ChannelID)
            .HasColumnName("channel_id")
            .IsRequired();
        
        builder.Property(p => p.GuildID)
            .HasColumnName("guild_id")
            .IsRequired(false);

        builder.Property(p => p.MessageID)
               .HasColumnName("message_id")
               .IsRequired(false);

        builder.Property(p => p.MessageContent)
               .HasColumnName("content")
               .IsRequired(false);
        
        builder.Property(p => p.ReplyMessageContent)
            .HasColumnName("reply_content")
            .IsRequired(false);
        
        builder.Property(p => p.ReplyAuthorID)
            .HasColumnName("reply_author_id")
            .IsRequired(false);
        
        builder.Property(p => p.ReplyMessageID)
            .HasColumnName("reply_message_id")
            .IsRequired(false);
    }
}