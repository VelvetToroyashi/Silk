using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Silk.Core.Data.Models;

namespace Silk.Core.Data.ModelConfigurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasKey(u => new {u.Id, u.GuildId});
            builder.HasOne(u => u.History)
                .WithOne(h => h.User)
                .HasForeignKey<UserHistory>(u => new {u.UserId, u.GuildId});
        }
    }
}