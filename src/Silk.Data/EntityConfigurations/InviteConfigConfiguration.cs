using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Silk.Data.Entities.Guild.Config;

namespace Silk.Data.EntityConfigurations;

public class InviteConfigConfiguration : IEntityTypeConfiguration<InviteConfigEntity>
{

    public void Configure(EntityTypeBuilder<InviteConfigEntity> builder)
    {
        builder.ToTable("invite_configs");
        
        builder.Property(p => p.WhitelistEnabled)
            .HasColumnName("whitelist_enabled");
        
        builder.Property(p => p.UseAggressiveRegex)
            .HasColumnName("use_aggressive_regex");

        builder.Property(p => p.WarnOnMatch)
               .HasColumnName("infract");
        
        builder.Property(p => p.DeleteOnMatch)
            .HasColumnName("delete");
        
        builder.Property(p => p.ScanOrigin)
            .HasColumnName("scan_origin");
    }
}