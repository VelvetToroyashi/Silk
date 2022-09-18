using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Silk.Data.Entities;

namespace Silk.Data.EntityConfigurations;

public class InfractionEntityConfiguration : IEntityTypeConfiguration<InfractionEntity>
{
    public void Configure(EntityTypeBuilder<InfractionEntity> builder)
    {
        builder.HasOne(inf => inf.Target)
               .WithMany(u => u.Infractions)
               .HasForeignKey(inf => inf.TargetID);

        builder.HasIndex(inf => inf.GuildID);
        builder.HasIndex(inf => inf.TargetID);

        builder.ToTable("infractions");

        builder.HasKey(p => p.ID);
        
        builder.Property(inf => inf.ID)
               .HasColumnName("id")
               .ValueGeneratedOnAdd()
               .IsRequired();
        
        builder.Property(p => p.TargetID)
               .HasColumnName("target_id")
               .IsRequired();

        builder.Property(p => p.GuildID)
               .HasColumnName("guild_id")
               .IsRequired();
        
        builder.Property(p => p.EnforcerID)
               .HasColumnName("enforcer_id")
               .IsRequired();

        builder.Property(p => p.CaseNumber)
               .HasColumnName("case_id")
               .IsRequired();

        builder.Property(p => p.Reason)
               .HasColumnName("reason")
               .IsRequired();
        
        builder.Property(p => p.UserNotified)
               .HasColumnName("user_notified")
               .HasDefaultValue(false)
               .IsRequired();
        
        builder.Property(p => p.Processed)
               .HasColumnName("processed")
               .HasDefaultValue(false)
               .IsRequired();

        builder.Property(p => p.CreatedAt)
               .HasColumnName("created_at")
               .IsRequired();
        
        builder.Property(p => p.Type)
               .HasColumnName("type")
               .IsRequired();
        
        builder.Property(p => p.Escalated)
               .HasColumnName("escalated")
               .HasDefaultValue(false)
               .IsRequired();
        
        builder.Property(p => p.AppliesToTarget)
               .HasColumnName("active")
               .HasDefaultValue(true)
               .IsRequired();
        
        builder.Property(p => p.ExpiresAt)
               .HasColumnName("expires_at")
               .IsRequired(false);
    }
}