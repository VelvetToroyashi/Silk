using Microsoft.EntityFrameworkCore;
using SilkBot.Models;

namespace SilkBot
{
    public class SilkDbContext : DbContext
    {
        public DbSet<DiscordUserInfo> DiscordUserInfoSet { get; set; }
        public DbSet<Guild> Guilds { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseNpgsql("Server=localhost;Database=Silk!;Username=postgres; Password=Cinnamon");

        protected override void OnModelCreating(ModelBuilder builder)
        {
            // Add user-to-post relationship
            builder.Entity<Ban>()
                .HasOne(b => b.UserInfo)
                
;

            base.OnModelCreating(builder);
        }
    }
}