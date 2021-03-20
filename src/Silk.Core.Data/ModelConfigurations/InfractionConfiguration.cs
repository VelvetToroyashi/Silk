using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Silk.Data.Models;

namespace Silk.Data.ModelConfigurations
{
    public class InfractionConfiguration : IEntityTypeConfiguration<Infraction>
    {

        public void Configure(EntityTypeBuilder<Infraction> builder)
        {
            builder.HasKey(i => i.Id);
        }
    }
}