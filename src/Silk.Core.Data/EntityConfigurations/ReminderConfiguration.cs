using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Silk.Core.Data.Entities;

namespace Silk.Core.Data.EntityConfigurations;

public class ReminderEntityConfiguration : IEntityTypeConfiguration<ReminderEntity>
{
    public void Configure(EntityTypeBuilder<ReminderEntity> builder)
    {
        builder.Property(rmr => rmr.ChannelID)
               .HasConversion<SnowflakeConverter>();
        
        builder.Property(rmr => rmr.OwnerID)
               .HasConversion<SnowflakeConverter>();

        builder.Property(rmr => rmr.MessageID)
               .HasConversion<NullableSnowflakeConverter>();
        
        builder.Property(rmr => rmr.GuildID)
               .HasConversion<NullableSnowflakeConverter>();

        builder.Property(rmr => rmr.ReplyAuthorID)
               .HasConversion<NullableSnowflakeConverter>();
        
        builder.Property(rmr => rmr.ReplyID)
               .HasConversion<NullableSnowflakeConverter>();
    }
}