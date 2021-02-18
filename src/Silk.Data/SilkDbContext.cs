using Microsoft.EntityFrameworkCore;
using Silk.Data.Models;

namespace Silk.Data
{
    public class SilkDbContext : DbContext
    {
        public DbSet<Guild> Guilds { get; set; } = null!;

        public DbSet<Ticket> Tickets { get; set; } = null!;

        /// <summary>
        /// Users on a guild level; holds information and states that reflect such.
        /// </summary>
        public DbSet<User> Users { get; set; } = null!;

        public DbSet<Infraction>? Infractions { get; set; } = null!;

        /// <summary>
        /// Users on a bot level; contains information that should have a globally persisted state.
        /// </summary>
        public DbSet<GlobalUser> GlobalUsers { get; set; } = null!;

        public DbSet<GuildConfig> GuildConfigs { get; set; } = null!;

        public SilkDbContext(DbContextOptions<SilkDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.ApplyConfigurationsFromAssembly(typeof(SilkDbContext).Assembly);
            builder.Entity<TicketMessage>().HasOne(ticket => ticket.Ticket).WithMany(ticket => ticket.History);
            builder.Entity<SelfAssignableRole>().Property(r => r.Id).ValueGeneratedNever();
            base.OnModelCreating(builder);
        }
    }
}