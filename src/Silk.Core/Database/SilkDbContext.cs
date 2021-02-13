
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Silk.Core.Database.Models;

namespace Silk.Core.Database
{
    public class SilkDbContext : DbContext
    {
        [NotNull]
        public DbSet<Guild> Guilds { get; set; }
        [NotNull]
        public DbSet<Ticket> Tickets { get; set; }
        //public DbSet<BaseShop> Shops { get; set; }
        [NotNull]
        public DbSet<Changelog> ChangeLogs { get; set; }
        [NotNull]
        public DbSet<Item> Items { get; set; }
        /// <summary>
        /// Users on a guild level; holds information and states that reflect such.
        /// </summary>
        [NotNull]
        public DbSet<User> Users { get; set; }
        
        public DbSet<Infraction> Infractions { get; set; }

        /// <summary>
        /// Users on a bot level; contains information that should have a globally persisted state.
        /// </summary>
        [NotNull]
        public DbSet<GlobalUser> GlobalUsers { get; set; }
        [NotNull]
        public DbSet<GuildConfig> GuildConfigs { get; set; }

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