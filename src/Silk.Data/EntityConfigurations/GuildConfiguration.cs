using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Silk.Data.Entities;

namespace Silk.Data.EntityConfigurations;

public class GuildEntityConfiguration : IEntityTypeConfiguration<GuildEntity>
{
    public void Configure(EntityTypeBuilder<GuildEntity> builder)
    {
        builder.HasKey(p => p.ID);
        
        builder
           .HasOne(g => g.Configuration)
           .WithOne(g => g.Guild)
           .HasForeignKey<GuildConfigEntity>(g => g.GuildID);

        builder.HasMany(u => u.Infractions)
               .WithOne(i => i.Guild)
               .HasForeignKey(i => i.GuildID);
        
        builder.ToTable("guilds");
        
        builder.Property(p => p.ID)
               .HasColumnName("id")
               .IsRequired();
        
        builder.Property(p => p.Prefix)
               .HasColumnName("prefix")
               .HasMaxLength(5)
               .IsRequired();
        
        
    }
}