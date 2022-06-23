using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Silk.Data.Entities;

namespace Silk.Data.EntityConfigurations;

public class UserHistoryEntityConfiguration : IEntityTypeConfiguration<UserHistoryEntity>
{

    public void Configure(EntityTypeBuilder<UserHistoryEntity> builder)
    {
        builder.HasKey(u => new { u.UserID, u.GuildID, u.Date });
        
        builder.HasIndex(u => u.UserID);
        builder.HasIndex(u => u.GuildID);
    }
}