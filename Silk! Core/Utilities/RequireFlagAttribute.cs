using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Microsoft.EntityFrameworkCore;
using SilkBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SilkBot.Utilities
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public sealed class RequireFlagAttribute : CheckBaseAttribute
    {
        public bool RequireGuild { get; }
        public UserFlag UserFlag { get; }
        private static readonly HashSet<ulong> _cachedStaff = new HashSet<ulong>();

        /// <summary>
        /// Check for a requisite flag from the database, and execute if check passes.
        /// </summary>
        /// <param name="RequireGuild">Restrict command usage to guild as well as requisite flag. Defaults to false.</param>
        public RequireFlagAttribute(UserFlag UserFlag, bool RequireGuild = false) { this.UserFlag = UserFlag; this.RequireGuild = RequireGuild; }
        public override async Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
        {

            if (ctx.Guild is null && RequireGuild) return false; //Is a private channel and requires a Guild//
            if (_cachedStaff.Contains(ctx.User.Id) && RequireGuild) return true;
            using var db = new SilkDbContext(); //Swap this for your own DBContext.//
            GuildModel guild = db.Guilds.Include(g => g.DiscordUserInfos).First(g => g.DiscordGuildId == ctx.Guild.Id);
            UserInfoModel member = guild.DiscordUserInfos.FirstOrDefault(m => m.UserId == ctx.User.Id);
            if (member is null) return false;
            if (member.Flags.HasFlag(UserFlag)) _cachedStaff.Add(member.UserId);
            return member.Flags.HasFlag(UserFlag);

        }
    }
}
