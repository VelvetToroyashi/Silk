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

        builder.ToTable("user_histories");

        builder.Property(p => p.UserID)
               .HasColumnName("user_id")
               .IsRequired();
        
        builder.Property(p => p.GuildID)
               .HasColumnName("guild_id")
               .IsRequired();

        builder.Property(p => p.Date)
               .HasColumnName("date")
               .IsRequired();
        
        builder.Property(p => p.IsJoin)
               .HasColumnName("is_join")
               .IsRequired();
    }
}