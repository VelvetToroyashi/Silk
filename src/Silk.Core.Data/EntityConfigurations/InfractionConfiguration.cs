using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Silk.Core.Data.Entities;

namespace Silk.Core.Data.EntityConfigurations;

public class InfractionEntityConfiguration : IEntityTypeConfiguration<InfractionEntity>
{
    public void Configure(EntityTypeBuilder<InfractionEntity> builder)
    {
        builder.HasOne(inf => inf.Target)
               .WithMany(u => u.Infractions)
               .HasForeignKey(inf => new {
                    TargetId = inf.TargetID,
                    GuildId  = inf.GuildID });
    }
}