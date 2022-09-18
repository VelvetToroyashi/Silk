using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Silk.Data.Entities;

namespace Silk.Data.EntityConfigurations;

public class GuildGreetingEntityConfiguration : IEntityTypeConfiguration<GuildGreetingEntity>
{

    public void Configure(EntityTypeBuilder<GuildGreetingEntity> builder)
    {
        builder.ToTable("guild_greetings");

        builder.HasOne(g => g.Guild);
        
        builder.Property(p => p.Id)
            .HasColumnName("id")
            .IsRequired();
        
        builder.Property(p => p.GuildID)
               .HasColumnName("guild_id")
               .IsRequired();
        
        builder.Property(p => p.Message)
               .HasColumnName("message")
               .IsRequired();
        
        builder.Property(p => p.Option)
               .HasColumnName("option")
               .IsRequired();
        
        builder.Property(p => p.ChannelID)
               .HasColumnName("channel_id")
               .IsRequired();

        builder.Property(p => p.MetadataID)
               .HasColumnName("metadata_id");
    }
}