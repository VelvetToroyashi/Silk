using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Silk.Data.Entities;

namespace Silk.Data.EntityConfigurations;

public class DisabledCommandEntityConfiguration : IEntityTypeConfiguration<DisabledCommandEntity>
{
    public void Configure(EntityTypeBuilder<DisabledCommandEntity> builder)
    {
        builder.HasIndex(c => new {
            GuildId = c.GuildID, c.CommandName }).IsUnique();
    }
}