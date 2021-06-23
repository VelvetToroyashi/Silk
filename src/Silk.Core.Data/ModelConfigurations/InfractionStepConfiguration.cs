using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NpgsqlTypes;
using Silk.Core.Data.Models;

namespace Silk.Core.Data.ModelConfigurations
{
	public class InfractionStepConfiguration : IEntityTypeConfiguration<InfractionStep>
	{

		public void Configure(EntityTypeBuilder<InfractionStep> builder)
		{
			builder.Property(infs => infs.Duration)
				.HasConversion(d => d.Ticks, d => NpgsqlTimeSpan.FromTicks(d));
		}
	}
}