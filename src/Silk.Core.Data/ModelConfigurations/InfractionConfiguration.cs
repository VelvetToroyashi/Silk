using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Silk.Core.Data.Models;

namespace Silk.Core.Data.ModelConfigurations
{
    public class InfractionConfiguration : IEntityTypeConfiguration<Infraction>
    {
        public void Configure(EntityTypeBuilder<Infraction> builder)
        {
            builder.HasOne(inf => inf.User)
                .WithMany(u => u.Infractions)
                .HasForeignKey(inf => new { inf.UserId, inf.GuildId});
        }
    }
}