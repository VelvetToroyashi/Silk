using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Silk.Core.Database.Models;

namespace Silk.Core.Database.ModelConfigurations.cs
{
    public class TicketResponderModelConfig : IEntityTypeConfiguration<TicketResponderModel>
    {

        public void Configure(EntityTypeBuilder<TicketResponderModel> builder) => builder.HasNoKey();
    }
}