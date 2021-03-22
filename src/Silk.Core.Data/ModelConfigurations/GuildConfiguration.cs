using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Silk.Core.Data.Models;

namespace Silk.Core.Data.ModelConfigurations
{
    public class GuildConfiguration : IEntityTypeConfiguration<Guild>
    {
        public void Configure(EntityTypeBuilder<Guild> builder)
        {
            builder.Property(g => g.Id).ValueGeneratedNever();

            builder
                .HasMany(u => u.Users)
                .WithOne(u => u.Guild);

            builder
                .HasOne(g => g.Configuration)
                .WithOne(g => g.Guild)
                .HasForeignKey<GuildConfig>(g => g.GuildId);
            
            builder.HasMany(u => u.Infractions).WithOne(i => i.Guild);
        }
    }
}