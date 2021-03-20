using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Silk.Core.Data.Models;

namespace Silk.Core.Data.ModelConfigurations
{
    public class InfractionConfiguration : IEntityTypeConfiguration<Infraction>
    {

        public void Configure(EntityTypeBuilder<Infraction> builder)
        {
            builder.HasKey(i => i.Id);
        }
    }
}