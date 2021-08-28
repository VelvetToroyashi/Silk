using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Silk.Api.Data.Entities;
using Silk.Api.Data.Models;

namespace Silk.Api.Data
{
	public class ApiContext : IdentityDbContext
	{
		public ApiContext(DbContextOptions options) : base(options) { }
		
		public DbSet<ApiUser> Users { get; set; }
		public DbSet<ApiKey> Keys { get; set; }
		public DbSet<Infraction> Infractions { get; set; }
		
	}
}