using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Silk.Data.Entities;

namespace Silk.Data.EntityConfigurations;

public class GuildEntityConfiguration : IEntityTypeConfiguration<GuildEntity>
{
    public void Configure(EntityTypeBuilder<GuildEntity> builder)
    {
        builder
           .HasOne(g => g.Configuration)
           .WithOne(g => g.Guild)
           .HasForeignKey<GuildConfigEntity>(g => g.GuildID);

        builder.HasMany(u => u.Infractions)
               .WithOne(i => i.Guild)
               .HasForeignKey(i => i.GuildID);
    }
}