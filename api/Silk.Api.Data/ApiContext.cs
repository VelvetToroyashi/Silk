using Microsoft.EntityFrameworkCore;
using Silk.Api.Data.Models;

namespace Silk.Api.Data
{
	public class ApiContext : DbContext
	{
		public DbSet<Infraction> Infractions { get; set; }
	}
}