using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Silk.Core.Data.Entities;

namespace Silk.Core.Data.EntityConfigurations;

public class InviteConfig : IEntityTypeConfiguration<InviteEntity>
{

    public void Configure(EntityTypeBuilder<InviteEntity> builder)
    {
        builder.Property(inv => inv.GuildId)
               .HasConversion(new SnowflakeConverter());
        
        builder.Property(inv => inv.InviteGuildId)
               .HasConversion(new SnowflakeConverter());
    }
}