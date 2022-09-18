using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Silk.Data.Entities;

namespace Silk.Data.EntityConfigurations;

public class UserEntityConfiguration : IEntityTypeConfiguration<UserEntity>
{
    public void Configure(EntityTypeBuilder<UserEntity> builder)
    {
        builder.HasKey(u => u.ID);

        builder.ToTable("users");

        builder.HasMany(u => u.History)
               .WithOne(u => u.User)
               .HasForeignKey(u => u.UserID);
        
        builder.Property(p => p.ID)
               .HasColumnName("id")
               .IsRequired();
        
        builder.Property(p => p.TimezoneID)
               .HasColumnName("timezone_id")
               .IsRequired(false);
        
        builder.Property(p => p.ShareTimezone)
               .HasColumnName("share_timezone")
               .IsRequired();
    }
}