using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Humanizer;
using Humanizer.Localisation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SilkBot.Commands.Bot;
using SilkBot.Extensions;
using SilkBot.Models;

namespace SilkBot.Utilities
{
    public class BotEventHelper
    {
        private readonly IDbContextFactory<SilkDbContext> _dbFactory;
        private readonly ILogger<BotEventHelper> _logger;
        private readonly DiscordShardedClient _client;
        private readonly Stopwatch _time = new();
        private readonly object _obj = new();
        private volatile bool _logged = false;
        private int _currentMemberCount = 0;
        private int expectedMembers;
        private int cachedMembers;


        public static List<Action> CacheStaff { get; } = new();
        public static Task GuildDownloadTask { get; private set; } = new(() => Task.Delay(-1));

        public BotEventHelper(DiscordShardedClient client, IDbContextFactory<SilkDbContext> dbFactory,
            ILogger<BotEventHelper> logger)
        {
            _dbFactory = dbFactory;
            _logger = logger;
            _client = client;
            _logger.LogInformation("Created Event Helper");
        }

        public void CreateHandlers()
        {
            _client.ClientErrored += OnClientErrored;
            foreach (CommandsNextExtension c in _client.GetCommandsNextAsync().GetAwaiter().GetResult().Values)
                c.CommandErrored += OnCommandErrored;
            _client.GuildAvailable += Cache;
            _logger.LogTrace("Subscribed to GUILD_AVAILABLE");
            _client.MessageDeleted += BotEvents.OnMessageDeleted;
            _logger.LogTrace("Subscribed to MESSAGE_DELETED");
            _client.GuildCreated += BotEvents.OnGuildJoin;
            _logger.LogTrace("Subscribed to GUILD_CREATED");
        }

        private async Task OnCommandErrored(CommandsNextExtension sender, CommandErrorEventArgs e)
        {
            if (e.Exception is not CommandNotFoundException)
            {
                if (e.Exception.Message is "Could not find a suitable overload for the command.") return;
                if (e.Exception is ArgumentException ex)
                    await e.Context.RespondAsync(string.IsNullOrEmpty(ex.Message)
                        ? "Seems like you provided an incorrect parameter."
                        : ex.Message);

                if (e.Exception is ChecksFailedException cf)
                    foreach (CheckBaseAttribute check in cf.FailedChecks)
                        switch (check)
                        {
                            case RequireOwnerAttribute:
                                await e.Context.RespondAsync($"You're not the owner of this bot!");
                                break;
                            case RequireNsfwAttribute:
                                await e.Context.RespondAsync("Channel must be marked as NSFW!");
                                break;
                            case RequireFlagAttribute flag:
                                await e.Context.RespondAsync(
                                    $"Sorry, but you need to have {flag.UserFlag} to run this command!");
                                break;
                            case CooldownAttribute cooldown:
                                await e.Context.RespondAsync(
                                    $"You're a bit too fast! Come back in {cooldown.GetRemainingCooldown(e.Context).Humanize(3, minUnit: TimeUnit.Second)}");
                                break;
                        }
                else await e.Context.RespondAsync($"See {e.Context.Prefix}help {e.Command.Name} for usage.");

                _logger.LogWarning(e.Exception, "");
            }
        }

        private async Task OnClientErrored(DiscordClient sender, ClientErrorEventArgs e)
        {
            if (e.Exception.Message.Contains("event"))
                _logger.LogWarning($"[{e.EventName}] Timed out.");
            else if (e.Exception.Message.ToLower().Contains("intents"))
                _logger.LogCritical("Intents aren't setup.");
        }


        private Task Cache(DiscordClient c, GuildCreateEventArgs e)
        {
            if (!_time.IsRunning)
            {
                _time.Start();
                _logger.LogTrace("Beginning Cache Run...");
            }

            _ = Task.Run(async () =>
            {
                using SilkDbContext db = _dbFactory.CreateDbContext();
                var sw = Stopwatch.StartNew();
                GuildModel guild = db.Guilds.AsQueryable().Include(g => g.Users)
                                     .FirstOrDefault(g => g.Id == e.Guild.Id);
                sw.Stop();
                _logger.LogTrace(
                    $"Retrieved guild from database in {sw.ElapsedMilliseconds} ms; guild {(guild is not null ? "does" : "does not")} exist.");

                if (guild is null)
                {
                    guild = new GuildModel {Id = e.Guild.Id, Prefix = Bot.SilkDefaultCommandPrefix};
                    db.Guilds.Add(guild);
                }

                sw.Restart();
                CacheStaffMembers(guild, e.Guild.Members.Values);

                await db.SaveChangesAsync();

                sw.Stop();
                if (sw.ElapsedMilliseconds > 400)
                    _logger.LogWarning(
                        $"Query took longer than allocated [250ms] time with tolerance of [150ms]. Query time: [{sw.ElapsedMilliseconds} ms]");
                _logger.LogDebug(
                    $"Shard [{c.ShardId + 1}/{c.ShardCount}] | Guild [{++_currentMemberCount}/{c.Guilds.Count}] | Time [{sw.ElapsedMilliseconds}ms]");
                if (_currentMemberCount == c.Guilds.Count && !_logged)
                {
                    _logged = true;
                    _time.Stop();
                    _logger.LogTrace("Cache run complete.");
                    _logger.LogDebug(
                        $"Expected [{expectedMembers}] members to be cached got [{cachedMembers}] instead. Cache run took {_time.ElapsedMilliseconds} ms.");
                }
            });
            return Task.CompletedTask;
        }


        private void CacheStaffMembers(GuildModel guild, IEnumerable<DiscordMember> members)
        {
            IEnumerable<DiscordMember> staff = members.Where(m => m.HasPermission(Permissions.MuteMembers) && !m.IsBot);
            lock (_obj)
            {
                expectedMembers += staff.Count();
            }

            foreach (DiscordMember member in staff)
            {
                var flags = UserFlag.Staff;
                if (member.HasPermission(Permissions.Administrator)) flags.Add(UserFlag.EscalatedStaff);

                UserModel user = guild.Users.FirstOrDefault(u => u.Id == member.Id);
                if (user is not null) //If user exists
                {
                    if (!user.Flags.Has(UserFlag.Staff)) // Has flag
                        user.Flags.Add(UserFlag.Staff); // Add flag
                    if (member.HasPermission(Permissions.Administrator))
                        user.Flags.Add(UserFlag.EscalatedStaff);
                }
                else
                {
                    guild.Users.Add(new UserModel {Id = member.Id, Flags = flags});
                    lock (_obj)
                    {
                        cachedMembers++;
                    }
                }
            }
        }
    }
}