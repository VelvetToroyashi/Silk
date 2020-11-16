using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SilkBot.Commands.Bot;
using SilkBot.Extensions;
using SilkBot.Models;
using System;
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

        public static List<Action> CacheStaff { get; } = new();
        public static Task GuildDownloadTask { get; private set; } = new(() => Task.Delay(-1));

        public BotEventHelper(DiscordShardedClient client, IDbContextFactory<SilkDbContext> dbFactory, ILogger<BotEventHelper> logger)
        {
            _dbFactory = dbFactory;
            _logger = logger;
            _client = client;
            _logger.LogInformation("Created Event Helper");
        }
        public void CreateHandlers()
        {

            _client.ClientErrored += (c, e) =>
            {
                e.Handled = true;
                if (e.Exception.Message.Contains("event handler")) _logger.LogError(e.Exception.Message + " " + e.EventName);//_logger.LogWarning($"An event handler timed out. [{e.EventName}]");
                else _logger.LogError($"An exception was thrown; message: {e.Exception.Message}");
                return Task.CompletedTask;
            };
            _client.GuildAvailable += OnGuildAvailable;
            _logger.LogTrace("Subcribed to GUILD_AVAILABLE event.");
            _client.MessageDeleted += BotEvents.OnMessageDeleted;
            _logger.LogTrace("Subscribed to MESSAGE_DELETED");
            _client.GuildCreated += BotEvents.OnGuildJoin;
        }


        private Task OnGuildAvailable(DiscordClient client, GuildCreateEventArgs e) => Cache(e, client);

        private async Task Cache(GuildCreateEventArgs e, DiscordClient c)
        {
            _ = Task.Run(async () => 
            {
                using var db = _dbFactory.CreateDbContext();
                GuildModel guild = GetOrCreateGuild(db, e.Guild.Id);
                    if (!db.Guilds.Any(g => g.Id == guild.Id)) db.Guilds.Add(guild);
                //await db.SaveChangesAsync();
                var sw = Stopwatch.StartNew();
                CacheStaffMembers(guild, e.Guild.Members.Values);
                db.Guilds.Update(guild);

                await db.SaveChangesAsync();
                sw.Stop();
                _logger.LogInformation($"Shard [{c.ShardId + 1}/{c.ShardCount}] | Cached [{++currentGuildCount}/{c.Guilds.Count}] guild in {sw.ElapsedMilliseconds} ms!");
                if (currentGuildCount == c.Guilds.Count) GuildDownloadTask = Task.CompletedTask;
            }).ConfigureAwait(false);


            //return Task.CompletedTask;
        }
        private static GuildModel GetOrCreateGuild(SilkDbContext db, ulong guildId)
        {
            var guild = db.Guilds.FirstOrDefault(g => g.Id == guildId);
            return guild ?? new GuildModel { Id = guildId, Prefix = Bot.SilkDefaultCommandPrefix };
        }



        private static void CacheStaffMembers(GuildModel guild, IEnumerable<DiscordMember> members)
        {
            var staffMembers = members
                .Where(member => member.HasPermission(Permissions.KickMembers) && !member.IsBot)
                .Select(staffMember => new UserModel { Id = staffMember.Id, Flags = UserFlag.Staff }).ToList();

            foreach (var staff in staffMembers)
            {
                
                UserModel user = guild.Users.FirstOrDefault(u => u.Id == staff.Id);
                if (user is not null) //If user exists
                {
                    if (!user.Flags.Has(UserFlag.Staff)) // Has flag
                            user.Flags.Add(UserFlag.Staff); // Add flag
                }
                else
                    guild.Users.Add(staff);
            }
        }
    }
}
