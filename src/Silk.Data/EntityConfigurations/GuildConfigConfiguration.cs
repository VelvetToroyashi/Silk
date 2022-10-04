using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Newtonsoft.Json;
using Silk.Data.Entities;
using Silk.Data.Entities.Guild.Config;

namespace Silk.Data.EntityConfigurations;

public class GuildConfigEntityConfiguration : IEntityTypeConfiguration<GuildConfigEntity>
{
    public void Configure(EntityTypeBuilder<GuildConfigEntity> builder)
    {
        builder.ToTable("guild_configs");
               
        builder.Property(p => p.GuildID)
               .HasColumnName("guild_id")
               .IsRequired();

        builder.Property(p => p.MuteRoleID)
               .HasColumnName("mute_role")
               .IsRequired();
        
        builder.Property(p => p.UseNativeMute)
               .HasColumnName("use_native_mute")
               .IsRequired();
        
        builder.Property(p => p.MaxUserMentions)
               .HasColumnName("max_user_mentions")
               .IsRequired();
        
        builder.Property(p => p.MaxRoleMentions)
               .HasColumnName("max_role_mentions")
               .IsRequired();
        
        builder.Property(p => p.ProgressiveStriking)
               .HasColumnName("progressive_striking")
               .IsRequired();
        
        builder.Property(p => p.DetectPhishingLinks)
               .HasColumnName("detect_phishing_links")
               .IsRequired();
        
        builder.Property(p => p.BanSuspiciousUsernames)
               .HasColumnName("ban_suspicious_usernames")
               .IsRequired();
        
        builder.Property(p => p.DeletePhishingLinks)
               .HasColumnName("delete_phishing_links")
               .IsRequired();
        
        builder.Property(p => p.EnableRaidDetection)
               .HasColumnName("detect_raids")
               .IsRequired();
        
        builder.Property(p => p.RaidDetectionThreshold)
               .HasColumnName("raid_detection_threshold")
               .IsRequired();
        
        builder.Property(p => p.RaidCooldownSeconds)
               .HasColumnName("raid_decay_seconds")
               .IsRequired();
        
        builder.Property(b => b.NamedInfractionSteps)
               .HasConversion(b => JsonConvert.SerializeObject(b,
                                                               new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }),
                              b => JsonConvert.DeserializeObject<Dictionary<string, InfractionStepEntity>>(b)!);

        builder.HasOne(c => c.Invites)
               .WithOne(c => c.GuildConfig)
               .HasForeignKey<InviteConfigEntity>(c => c.GuildModConfigId);
    }
}