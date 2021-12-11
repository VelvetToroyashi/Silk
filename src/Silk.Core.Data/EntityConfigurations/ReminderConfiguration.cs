using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Silk.Core.Data.Entities;

namespace Silk.Core.Data.EntityConfigurations;

public class ReminderEntityConfiguration : IEntityTypeConfiguration<ReminderEntity>
{
    public void Configure(EntityTypeBuilder<ReminderEntity> builder)
    {
        builder.Property(rmr => rmr.ChannelID)
               .HasConversion(new SnowflakeConverter());
        
        builder.Property(rmr => rmr.OwnerID)
               .HasConversion(new SnowflakeConverter());

        builder.Property(rmr => rmr.MessageID)
               .HasConversion(new NullableSnowflakeConverter());
        
        builder.Property(rmr => rmr.GuildID)
               .HasConversion(new NullableSnowflakeConverter());

        builder.Property(rmr => rmr.ReplyAuthorID)
               .HasConversion(new NullableSnowflakeConverter());
        
        builder.Property(rmr => rmr.ReplyID)
               .HasConversion(new NullableSnowflakeConverter());
    }
}