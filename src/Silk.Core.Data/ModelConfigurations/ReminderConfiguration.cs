using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Silk.Core.Data.Models;

namespace Silk.Core.Data.ModelConfigurations
{
    public class ReminderConfiguration : IEntityTypeConfiguration<Reminder>
    {
        public void Configure(EntityTypeBuilder<Reminder> builder)
        {
            builder.HasKey(r => r.Id);
            builder.HasOne(r => r.Owner)
                .WithMany(u => u.Reminders)
                .HasPrincipalKey(u => new {u.Id, u.GuildId})
                .HasForeignKey(r => new {r.OwnerId, r.GuildId});
        }
    }
}