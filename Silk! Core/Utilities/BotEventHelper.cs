using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SilkBot.Commands.Bot;
using SilkBot.Commands.Moderation.Utilities;
using SilkBot.Extensions;
using SilkBot.Models;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace SilkBot.Utilities
{
    public class BotEventHelper
    {

        private readonly IDbContextFactory<SilkDbContext> _dbFactory;
        private readonly ILogger<BotEventHelper> _logger;
        private readonly DiscordShardedClient _client;
        private int currentGuildCount = 0;

        public BotEventHelper(DiscordShardedClient client, IDbContextFactory<SilkDbContext> dbFactory, ILogger<BotEventHelper> logger)
        {
            _dbFactory = dbFactory;
            _logger = logger;
            _client = client;
            CreateHandlers(_client);
        }
        public void CreateHandlers(DiscordShardedClient Client)
        {
            Client.ClientErrored += (c, e) =>
            {
                e.Handled = true;
                if (e.Exception.Message.Contains("event handler")) _logger.LogWarning("An event handler timed out.");
                else _logger.LogError($"An exception was thrown; message: {e.Exception.Message}");
                return Task.CompletedTask;
            };
            Client.GuildAvailable += OnGuildAvailable;
            Client.MessageDeleted += new MessageDeletionHandler().OnMessageDeleted;
            Client.GuildCreated += new GuildJoinHandler().OnGuildJoin;
        }


        private async Task OnGuildAvailable(DiscordClient sender, GuildCreateEventArgs e)

        {
            var db = _dbFactory.CreateDbContext();
            var guild = await GetOrCreateGuildAsync(db, e.Guild.Id);
            if (!db.Guilds.Any(g => g.Id == guild.Id)) db.Guilds.Add(guild);
            var sw = Stopwatch.StartNew();
            CacheStaffMembers(guild, e.Guild.Members.Values);
            db.Guilds.Update(guild);
            await db.SaveChangesAsync();
            sw.Stop();
            _logger.LogInformation($"Cached [{++currentGuildCount}/{sender.Guilds.Count}] guild in {sw.ElapsedMilliseconds} ms!");
        }

        public async Task<GuildModel> GetOrCreateGuildAsync(SilkDbContext db, ulong guildId)
        {
            var guild = await db.Guilds.FirstOrDefaultAsync(g => g.Id == guildId);
            return guild ?? new GuildModel { Id = guildId, Prefix = Bot.SilkDefaultCommandPrefix };
        }



        private void CacheStaffMembers(GuildModel guild, IEnumerable<DiscordMember> members)
        {
            var staffMembers = members
                .Where(member => member.HasPermission(Permissions.KickMembers) && !member.IsBot)
                .Select(staffMember => new UserInfoModel { UserId = staffMember.Id, Flags = UserFlag.Staff });

            var users = guild.DiscordUserInfos;
            foreach (var staff in staffMembers)
            {
                if (users.Any(u => u.UserId == staff.UserId && u.Flags.HasFlag(UserFlag.Staff))) continue;
                else if (users.Any(u => u.UserId == staff.UserId))
                {
                    _logger.LogTrace("Added staff flag to user");
                    var user = users.Single(u => u.UserId == staff.UserId);
                    user.Flags.Add(UserFlag.Staff);
                }
                else users.Add(staff);
            }
        }
    }
}
