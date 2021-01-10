using Microsoft.EntityFrameworkCore;
using Silk.Core.Database.Models;

namespace Silk.Core.Database
{
    public class SilkDbContext : DbContext
    {
        public DbSet<GuildModel> Guilds { get; set; }

        public DbSet<TicketModel> Tickets { get; set; }
        //public DbSet<BaseShop> Shops { get; set; }
        
        public DbSet<ChangelogModel> ChangeLogs { get; set; }
        
        public DbSet<ItemModel> Items { get; set; }
        /// <summary>
        /// Users on a guild level; holds information and states that reflect such.
        /// </summary>
        public DbSet<UserModel> Users { get; set; }


        /// <summary>
        /// Users on a bot level; contains information that should have a globally persisted state.
        /// </summary>
        public DbSet<GlobalUserModel> GlobalUsers { get; set; }
        public DbSet<GuildConfigModel> GuildConfigs { get; set; }
        
        public SilkDbContext(DbContextOptions<SilkDbContext> options) : base(options) { }


        // TODO: Clean this up
        
        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<UserModel>().HasOne(g => g.Guild);
            
            builder.Entity<UserInfractionModel>().HasIndex(a => a.Id);
            builder.Entity<GuildModel>().HasOne(g => g.Configuration).WithOne(g => g.Guild).HasForeignKey<GuildConfigModel>(g => g.GuildId);

            builder.Entity<GuildModel>().HasKey(g => g.Id);
            builder.Entity<GuildModel>().Property(g => g.Id).ValueGeneratedNever();

            builder.Entity<GuildConfigModel>().HasKey(c => c.ConfigId);
            builder.Entity<SelfAssignableRole>().HasKey(r => r.RoleId);
            builder.Entity<UserModel>().HasMany(u => u.Infractions).WithOne(i => i.User);
            builder.Entity<GuildModel>().HasMany(u => u.Users);
            builder.Entity<TicketResponderModel>().HasNoKey();
            builder.Entity<TicketMessageHistoryModel>().HasOne(ticket => ticket.TicketModel)
                   .WithMany(ticket => ticket.History);
            builder.Entity<SelfAssignableRole>().Property(r => r.RoleId).ValueGeneratedNever();
            base.OnModelCreating(builder);
        }
    }
}