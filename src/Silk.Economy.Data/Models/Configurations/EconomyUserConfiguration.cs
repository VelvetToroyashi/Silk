using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Silk.Economy.Data.Models.Configurations
{
	public class EconomyUserConfiguration : IEntityTypeConfiguration<EconomyUser>
	{
		public void Configure(EntityTypeBuilder<EconomyUser> builder)
		{
			builder.HasKey(x => x.UserId);
			
			builder.HasMany(u => u.Transactions);
		}
		
		
	}
}