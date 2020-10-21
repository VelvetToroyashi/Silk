using Microsoft.EntityFrameworkCore;
using SilkBot.Database.Models;
using SilkBot.Models;
using System.IO;

namespace SilkBot
{
    public class SilkDbContext : DbContext
    {
        public DbSet<Guild> Guilds { get; set; }
        public DbSet<TicketModel> Tickets { get; set; }
        //public DbSet<BaseShop> Shops { get; set; }
        public DbSet<ChangelogModel> ChangeLogs { get; set; }
        public DbSet<DiscordUserInfo> Users { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            var sqlString = File.ReadAllText("./dbLogin");
            options.EnableDetailedErrors();
            options.UseNpgsql(sqlString);
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            
            builder.Entity<Ban>().HasOne(b => b.UserInfo);
            builder.Entity<WhiteListedLink>().HasOne(w => w.Guild).WithMany(a => a.WhiteListedLinks);
            builder.Entity<BlackListedWord>().HasOne(_ => _.Guild).WithMany(g => g.BlackListedWords);
            //builder.Entity<UserInfractionModel>().HasOne(i => i.User);
            builder.Entity<TicketResponderModel>().HasNoKey();
            builder.Entity<TicketMessageHistoryModel>().HasOne(ticket => ticket.TicketModel).WithMany(ticket => ticket.History);
            base.OnModelCreating(builder);
        }
    }
}