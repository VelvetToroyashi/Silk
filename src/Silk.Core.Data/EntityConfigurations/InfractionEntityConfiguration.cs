using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Silk.Core.Data.Entities;

namespace Silk.Core.Data.EntityConfigurations
{
    public class InfractionEntityConfiguration : IEntityTypeConfiguration<InfractionEntity>
    {
        public void Configure(EntityTypeBuilder<InfractionEntity> builder)
        {
            builder.HasOne(inf => inf.User)
                .WithMany(u => u.Infractions)
                .HasForeignKey(inf => new { inf.UserId, inf.GuildId });
        }
    }
}