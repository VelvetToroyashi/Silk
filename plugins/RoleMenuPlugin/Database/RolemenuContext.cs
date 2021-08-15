using Microsoft.EntityFrameworkCore;

namespace RoleMenuPlugin.Database
{
	public sealed class RolemenuContext : DbContext
	{
		public DbSet<RoleMenuModel> RoleMenus { get; set; }
		public RolemenuContext(DbContextOptions options) : base(options) { }
	}
}