using Microsoft.EntityFrameworkCore;

namespace RoleMenuPlugin.Database
{
    public sealed class RoleMenuContext : DbContext
    {
        public RoleMenuContext(DbContextOptions options) : base(options) { }
        public DbSet<RoleMenuModel> RoleMenus { get; set; }
    }
}