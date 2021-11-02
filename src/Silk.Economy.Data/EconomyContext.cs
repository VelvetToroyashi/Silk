using Microsoft.EntityFrameworkCore;
using Silk.Economy.Data.Models;

namespace Silk.Economy.Data
{
	public class EconomyContext : DbContext
	{
		public DbSet<EconomyUser> EconomyUsers { get; set; }
		public DbSet<EconomyTransaction> EconomyTransactions { get; set; }
		
		public EconomyContext(DbContextOptions<EconomyContext> options) : base(options) { }
		
		protected override void OnModelCreating(ModelBuilder builder)
		{
			builder.ApplyConfigurationsFromAssembly(typeof(EconomyContext).Assembly);
			base.OnModelCreating(builder);
		}
	}
}