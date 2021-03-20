using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Silk.Core.Data.Models;

namespace Silk.Core.Data.ModelConfigurations
{
    public class TicketResponderModelConfig : IEntityTypeConfiguration<TicketResponder>
    {

        public void Configure(EntityTypeBuilder<TicketResponder> builder) => builder.HasNoKey();
    }
}