using Microsoft.EntityFrameworkCore;
using SilkBot.Database.Models;
using SilkBot.Models;

namespace SilkBot
{
    public class SilkDbContext : DbContext
    {

        public DbSet<GuildModel> Guilds { get; set; }
        public DbSet<TicketModel> Tickets { get; set; }
        //public DbSet<BaseShop> Shops { get; set; }
        public DbSet<ChangelogModel> ChangeLogs { get; set; }

        public DbSet<UserModel> Users { get; set; }
        public DbSet<GlobalUserModel> GlobalUsers { get; set; }

        public SilkDbContext(DbContextOptions<SilkDbContext> options) : base(options) { }


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