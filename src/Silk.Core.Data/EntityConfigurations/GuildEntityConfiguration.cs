using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Silk.Core.Data.Entities;

namespace Silk.Core.Data.EntityConfigurations;

public class GuildEntityConfiguration : IEntityTypeConfiguration<GuildEntity>
{
    public void Configure(EntityTypeBuilder<GuildEntity> builder)
    {
        builder.Property(g => g.Id).ValueGeneratedNever();

        builder
           .HasMany(u => u.Users)
           .WithOne(u => u.Guild);

        builder
           .HasOne(g => g.Configuration)
           .WithOne(g => g.Guild)
           .HasForeignKey<GuildConfigEntity>(g => g.GuildId);

        builder
           .HasOne(g => g.ModConfig)
           .WithOne(g => g.Guild)
           .HasForeignKey<GuildModConfigEntity>(g => g.GuildId);

        builder.HasMany(u => u.Infractions).WithOne(i => i.Guild);
    }
}