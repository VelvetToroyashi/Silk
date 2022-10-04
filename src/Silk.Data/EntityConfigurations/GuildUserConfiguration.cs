using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Silk.Data.Entities;

namespace Silk.Data.EntityConfigurations;

public class GuildUserConfiguration : IEntityTypeConfiguration<GuildUserEntity>
{
    public void Configure(EntityTypeBuilder<GuildUserEntity> builder)
    {
        builder.HasKey(gu => new { gu.UserID, gu.GuildID });

        builder.HasOne(gu => gu.User)
               .WithMany(u => u.Guilds)
               .HasForeignKey(u => u.UserID);
        
        builder.HasOne(gu => gu.Guild)
               .WithMany(g => g.Users)
               .HasForeignKey(g => g.GuildID);

        builder.ToTable("guild_user_joiner");
        
        builder.Property(gu => gu.UserID)
               .HasColumnName("user_id")
               .IsRequired();
        
        builder.Property(gu => gu.GuildID)
               .HasColumnName("guild_id")
               .IsRequired();
    }
}