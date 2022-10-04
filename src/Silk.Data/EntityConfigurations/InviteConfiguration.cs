using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Silk.Data.Entities;

namespace Silk.Data.EntityConfigurations;

public class InviteConfiguration : IEntityTypeConfiguration<InviteEntity>
{
    public void Configure(EntityTypeBuilder<InviteEntity> builder)
    {
        builder.ToTable("invites");
        
        builder.HasKey(x => x.ID);
        
        builder.Property(x => x.ID)
            .HasColumnName("id")
            .IsRequired();
        
        builder.Property(p => p.GuildId)
            .HasColumnName("guild_id")
            .IsRequired();
        
        builder.Property(p => p.InviteGuildId)
            .HasColumnName("invite_guild_id")
            .IsRequired();

        builder.Property(p => p.VanityURL)
               .HasColumnName("invite_code");
    }
}