using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Silk.Core.Database.Models;

namespace Silk.Core.Database.ModelConfigurations.cs
{
    public class TicketResponderModelConfig : IEntityTypeConfiguration<TicketResponder>
    {

        public void Configure(EntityTypeBuilder<TicketResponder> builder) => builder.HasNoKey();
    }
}