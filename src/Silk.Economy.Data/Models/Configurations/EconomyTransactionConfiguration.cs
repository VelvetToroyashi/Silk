using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Silk.Economy.Data.Models.Configurations
{
	public class EconomyTransactionConfiguration : IEntityTypeConfiguration<EconomyTransaction>
	{

		public void Configure(EntityTypeBuilder<EconomyTransaction> builder)
		{
			builder.HasKey(x => x.Id);
			
		}
	}
}