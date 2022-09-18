using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Silk.Data.Entities;

namespace Silk.Data.EntityConfigurations;

public class PendingGreetingConfiguration : IEntityTypeConfiguration<PendingGreetingEntity>
{
    public void Configure(EntityTypeBuilder<PendingGreetingEntity> builder)
    {
        builder.ToTable("pending_greetings");
        
        builder.HasKey(x => x.ID);
        
        builder.Property(x => x.ID)
            .HasColumnName("id")
            .IsRequired();
        
        builder.Property(p => p.GuildID)
            .HasColumnName("guild_id")
            .IsRequired();
        
        builder.Property(p => p.UserID)
            .HasColumnName("user_id")
            .IsRequired();
        
        builder.Property(p => p.GreetingID)
            .HasColumnName("greeting_id")
            .IsRequired();
    }
}