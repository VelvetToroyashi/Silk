using System.Collections.Generic;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Silk.Core.Data.Models
{
    public class RoleMenu
    {
        public int Id { get; set; }
        public ulong MessageId { get; set; }

        public int GuildConfigId { get; set; }
        public Dictionary<string, ulong> RoleDictionary { get; set; }
    }

    public class RoleMenuConfiguration : IEntityTypeConfiguration<RoleMenu>
    {
        public void Configure(EntityTypeBuilder<RoleMenu> builder)
        {
            builder.Property(r => r.RoleDictionary)
                .HasConversion(v => JsonSerializer.Serialize(v, new()),
                    v => JsonSerializer.Deserialize<Dictionary<string, ulong>>(v, new())!);
        }
    }
}