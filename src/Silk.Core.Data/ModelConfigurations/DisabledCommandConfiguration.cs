using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Silk.Core.Data.Models;

namespace Silk.Core.Data.ModelConfigurations
{
    public class DisabledCommandConfiguration : IEntityTypeConfiguration<DisabledCommand>
    {

        public void Configure(EntityTypeBuilder<DisabledCommand> builder)
        {
            builder.HasIndex(c => new { c.GuildId, c.CommandName }).IsUnique();
        }
    }
}