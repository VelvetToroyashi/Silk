using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Silk.Core.Data.Entities;

namespace Silk.Core.Data.EntityConfigurations
{
    public class UserEntityConfiguration : IEntityTypeConfiguration<UserEntity>
    {
        public void Configure(EntityTypeBuilder<UserEntity> builder)
        {
            builder.HasKey(u => new { u.Id, u.GuildId });
            builder.HasOne(u => u.History)
                   .WithOne(h => h.User)
                   .HasForeignKey<UserHistoryEntity>(u => new { u.UserId, u.GuildId });
        }
    }
}