using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Silk.Data.Models;

namespace Silk.Data.ModelConfigurations.cs
{
    public class TicketResponderModelConfig : IEntityTypeConfiguration<TicketResponder>
    {

        public void Configure(EntityTypeBuilder<TicketResponder> builder) => builder.HasNoKey();
    }
}