using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Silk.Core.Data.Models;

namespace Silk.Core.Data.ModelConfigurations
{
    public class InfractionConfiguration : IEntityTypeConfiguration<Infraction>
    {

        public void Configure(EntityTypeBuilder<Infraction> builder)
        {
            builder.HasKey(inf => new {inf.Id, inf.GuildId});
            builder.Property(inf => inf.Id).ValueGeneratedOnAdd();
        }
    }
}