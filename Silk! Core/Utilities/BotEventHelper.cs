using Colorful;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.EntityFrameworkCore;
using Silk__Extensions;
using SilkBot.Commands.Bot;
using SilkBot.Commands.Moderation.Utilities;
using SilkBot.Models;
using SilkBot.Server;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace SilkBot.Utilities
{
    public class BotEventHelper
    {
        private List<Action<Guild, IEnumerable<DiscordMember>, SilkDbContext>> actions = new List<Action<Guild, IEnumerable<DiscordMember>, SilkDbContext>>();
        private readonly IDbContextFactory<SilkDbContext> _dbFactory;

        public BotEventHelper(IDbContextFactory<SilkDbContext> dbFactory) => _dbFactory = dbFactory;
        public void CreateHandlers(DiscordClient Client)
        {
            Client.Ready += (c, e) => 
            {
                System.Console.WriteLine("Client ready."); 
                return Task.CompletedTask; 
            };
            Client.GuildAvailable += OnGuildAvailable;
            Client.MessageDeleted += new MessageDeletionHandler().OnMessageDeleted;
            Client.MessageUpdated += new MessageEditHandler().OnMessageEdit;
            Client.GuildCreated += new GuildJoinHandler().OnGuildJoin;
            var memberCountChangeHandler = new GuildMemberCountChangeHandler();
            Client.GuildMemberAdded += memberCountChangeHandler.OnGuildMemberJoined;
            Client.GuildMemberRemoved += memberCountChangeHandler.OnGuildMemberLeft;
        }

        private Task OnGuildAvailable(DiscordClient sender, GuildCreateEventArgs e)
        {
                var db = new SilkDbContext();
                var guild = GetOrCreateGuildAsync(e.Guild.Id).GetAwaiter().GetResult();
                if (!db.Guilds.Contains(guild)) db.Guilds.Add(guild);
                CacheStaffMembers(db, guild, e.Guild.Members.Values).GetAwaiter();
                db.SaveChangesAsync().GetAwaiter();
                Colorful.Console.WriteLine($"Cached guild! {e.Guild.Name} - {e.Guild.Owner.DisplayName} - {e.Guild.MemberCount}", Color.LightGreen);

                Colorful.Console.ForegroundColor = Color.White;

            return Task.CompletedTask;
        }

        public async Task<Guild> GetOrCreateGuildAsync(ulong guildId)
        {
            var db =_dbFactory.CreateDbContext();
            var guild = await db.Guilds.FirstOrDefaultAsync(g => g.DiscordGuildId == guildId);

            if (guild != null)
            {
                return guild;
            }

            guild = new Guild { DiscordGuildId = guildId, Prefix = Bot.SilkDefaultCommandPrefix };
            return guild;
        }



        public async Task CacheStaffMembers(SilkDbContext db, Guild guild, IEnumerable<DiscordMember> members)
        {
            var staffMembers = members
                .Where(member => member.HasPermission(Permissions.KickMembers | Permissions.All) && !member.IsBot)
                .Select(staffMember => new UserInfoModel { Guild = guild, UserId = staffMember.Id, Flags = UserFlag.Staff });


            guild.DiscordUserInfos.AddRange(staffMembers);
            await db.SaveChangesAsync();
        }


    }
}
