using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Silk.Data.Entities;

namespace Silk.Data.EntityConfigurations;

public class LoggingConfigConfiguration : IEntityTypeConfiguration<GuildLoggingConfigEntity>
{

    public void Configure(EntityTypeBuilder<GuildLoggingConfigEntity> builder)
    {
        builder
           .Property(p => p.UseMobileFriendlyLogging)
           .ValueGeneratedNever()
           .HasDefaultValue(true);
    }
}