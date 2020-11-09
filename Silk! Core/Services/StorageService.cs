using Microsoft.EntityFrameworkCore;
using SilkBot.Models;
using System;
using System.Linq;

namespace SilkBot.Services
{
    public class StorageService
    {
        private readonly IDbContextFactory<SilkDbContext> _dbFactory;

        public GuildModel GetGuild(ulong Id)
        {
            using var db = _dbFactory.CreateDbContext();
            return db.Guilds.FirstOrDefault(g => g.Id == Id);
        }


        public UserInfoModel GetUserById(GuildModel guild, ulong userId) => guild.DiscordUserInfos.FirstOrDefault(u => u.UserId == userId);
        public UserInfoModel GetUserById(Func<GuildModel, bool> guild, ulong userId)
        {
            using var db = _dbFactory.CreateDbContext();
            return db.Guilds.FirstOrDefault(guild).DiscordUserInfos.FirstOrDefault(u => u.UserId == userId);
        }


        public UserInfoModel GetUser(GuildModel guild, UserInfoModel userId) => guild.DiscordUserInfos.FirstOrDefault(u => u.UserId == userId.UserId);
        public UserInfoModel GetUser(Func<GuildModel, bool> guild, Func<UserInfoModel, bool> user)
        {
            using var db = _dbFactory.CreateDbContext();
            return db.Guilds.FirstOrDefault(guild).DiscordUserInfos.FirstOrDefault(user);
        }


    }
}
