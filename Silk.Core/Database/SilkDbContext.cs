
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Silk.Core.Database.Models;

namespace Silk.Core.Database
{
    public class SilkDbContext : DbContext
    {
        [NotNull]
        public DbSet<GuildModel> Guilds { get; set; }
        [NotNull]
        public DbSet<TicketModel> Tickets { get; set; }
        //public DbSet<BaseShop> Shops { get; set; }
        [NotNull]
        public DbSet<ChangelogModel> ChangeLogs { get; set; }
        [NotNull]
        public DbSet<ItemModel> Items { get; set; }
        /// <summary>
        /// Users on a guild level; holds information and states that reflect such.
        /// </summary>
        [NotNull]
        public DbSet<UserModel> Users { get; set; }


        /// <summary>
        /// Users on a bot level; contains information that should have a globally persisted state.
        /// </summary>
        [NotNull]
        public DbSet<GlobalUserModel> GlobalUsers { get; set; }
        [NotNull]
        public DbSet<GuildConfigModel> GuildConfigs { get; set; }

        public SilkDbContext(DbContextOptions<SilkDbContext> options) : base(options) { }
        
        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<TicketMessageHistoryModel>().HasOne(ticket => ticket.TicketModel).WithMany(ticket => ticket.History);
            builder.Entity<SelfAssignableRole>().Property(r => r.Id).ValueGeneratedNever();
            base.OnModelCreating(builder);
        }
    }
}