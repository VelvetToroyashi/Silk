using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Newtonsoft.Json;
using Silk.Core.Data.Models;

namespace Silk.Core.Data.ModelConfigurations
{
	public sealed class GuildModConfigConfiguration : IEntityTypeConfiguration<GuildModConfig>
	{
		public void Configure(EntityTypeBuilder<GuildModConfig> builder)
		{
			builder.Property(b => b.NamedInfractionSteps)
				.HasConversion(b => JsonConvert.SerializeObject(b),
					b => JsonConvert.DeserializeObject<Dictionary<string, InfractionStep>>(b));
		}
	}
}