using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Silk.Core.Data.Entities;

namespace Silk.Core.Data.EntityConfigurations;

public class DisabledCommandEntityConfiguration : IEntityTypeConfiguration<DisabledCommandEntity>
{
    public void Configure(EntityTypeBuilder<DisabledCommandEntity> builder)
    {
        builder.HasIndex(c => new { c.GuildId, c.CommandName }).IsUnique();
    }
}