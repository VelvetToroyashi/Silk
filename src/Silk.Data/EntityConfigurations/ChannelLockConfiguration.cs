using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Remora.Discord.API;
using Silk.Data.Entities.Channels;

namespace Silk.Data.EntityConfigurations;

public class ChannelLockConfiguration : IEntityTypeConfiguration<ChannelLockEntity>
{
    public void Configure(EntityTypeBuilder<ChannelLockEntity> builder)
    {
        builder.Property(cl => cl.LockedRoles).HasPostgresArrayConversion(sf => sf.Value, sf => DiscordSnowflake.New(sf));
    }
}