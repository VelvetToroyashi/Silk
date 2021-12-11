using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Silk.Core.Data.Entities;

namespace Silk.Core.Data.EntityConfigurations;

public class UserHistoryConfiguration : IEntityTypeConfiguration<UserHistoryEntity>
{
    public void Configure(EntityTypeBuilder<UserHistoryEntity> builder)
    {
        builder.Property(uh => uh.GuildID)
               .HasConversion(new SnowflakeConverter());

        builder.Property(uh => uh.UserID)
               .HasConversion(new SnowflakeConverter());
    }
}