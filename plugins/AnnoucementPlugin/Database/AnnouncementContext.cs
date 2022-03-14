using Microsoft.EntityFrameworkCore;

namespace AnnoucementPlugin.Database
{
    public sealed class AnnouncementContext : DbContext
    {

        public AnnouncementContext(DbContextOptions options) : base(options) { }
        public DbSet<AnnouncementModel> Announcements { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("announcement_plugin");
            base.OnModelCreating(modelBuilder);
        }
    }
}