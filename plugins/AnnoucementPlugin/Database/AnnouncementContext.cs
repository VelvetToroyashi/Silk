using Microsoft.EntityFrameworkCore;

namespace AnnoucementPlugin.Database
{
	public sealed class AnnouncementContext : DbContext
	{
		public DbSet<AnnouncementModel> Announcements { get; set; }

		public AnnouncementContext(DbContextOptions options) : base(options) { }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.HasDefaultSchema("Announcement-Plugin");
			base.OnModelCreating(modelBuilder);
		}
	}
}