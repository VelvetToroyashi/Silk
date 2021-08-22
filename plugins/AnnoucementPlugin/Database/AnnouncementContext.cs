using Microsoft.EntityFrameworkCore;

namespace AnnoucementPlugin.Database
{
	public sealed class AnnouncementContext : DbContext
	{
		public DbSet<AnnouncementModel> Announcements { get; set; }

		public AnnouncementContext(DbContextOptions options) : base(options) { }
	}
}