using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
using SilkBot.Database;
using SilkBot.Database.Models;
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
        private bool _hasLoggedCompletion;
        private int _currentMemberCount;
        private int expectedMembers;
        private int cachedMembers;
        private int guildMembers;
        public BotEventHelper(DiscordShardedClient client, IDbContextFactory<SilkDbContext> dbFactory,
            ILogger<BotEventHelper> logger)
        {
            _dbFactory = dbFactory;
            _logger = logger;
            _client = client;
        }

        public void CreateHandlers()
        {
            _client.ClientErrored += OnClientErrored;
            foreach (CommandsNextExtension c in _client.GetCommandsNextAsync().GetAwaiter().GetResult().Values)
                c.CommandErrored += OnCommandErrored;
        }

        private async Task OnCommandErrored(CommandsNextExtension sender, CommandErrorEventArgs e)
        {
            
            if (e.Exception is CommandNotFoundException)
            {
                _logger.LogWarning($"Unkown command: {e.Command.Name}. Arguments: {e.Context.RawArgumentString}");
                return;
            }

            if (e.Exception.Message is "No matching subcommands were found, and this group is not executable.") 
            {
                _logger.LogWarning($"Unknown subcommand: {e.Command.Name} | Arguments: {e.Context.RawArgumentString}");
                return;
            }
            
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
            _logger.LogWarning(e.Exception, "");
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
                guildMembers += e.Guild.MemberCount;
                
                using SilkDbContext db = _dbFactory.CreateDbContext();
                var sw = Stopwatch.StartNew();
                GuildModel? guild = db.Guilds.AsQueryable().Include(g => g.Users)
                                     .FirstOrDefault(g => g.Id == e.Guild.Id);
                sw.Stop();
                _logger.LogTrace($"Retrieved guild from database in {sw.ElapsedMilliseconds} ms.");

                if (guild is null)
                {
                    guild = new (){Id = e.Guild.Id, Prefix = Bot.DefaultCommandPrefix};
                    db.Guilds.Add(guild);
                }

                sw.Restart();
                CacheStaffMembers(guild, e.Guild.Members.Values);

                await db.SaveChangesAsync();

                sw.Stop();
                if (sw.ElapsedMilliseconds > 300)
                    _logger.LogWarning($"Databse query took longer than expected. (Expected <300ms, took {sw.ElapsedMilliseconds} ms)");
                _logger.LogDebug(
                    $"Shard [{c.ShardId + 1}/{c.ShardCount}] | Guild [{++_currentMemberCount}/{c.Guilds.Count}] | {sw.ElapsedMilliseconds}ms");
                if (_currentMemberCount == c.Guilds.Count && !_hasLoggedCompletion)
                {
                    _hasLoggedCompletion = true;
                    _time.Stop();
                    _logger.LogTrace("Cache run complete.");
                    if(expectedMembers < cachedMembers) _logger.LogWarning($"{expectedMembers} members flagged as staff from iterating over {guildMembers} members. [{cachedMembers}/{expectedMembers}] saved to db.");
                }
            });
            return Task.CompletedTask;
        }


        private void CacheStaffMembers(GuildModel guild, IEnumerable<DiscordMember> members)
        {
            IEnumerable<DiscordMember> staff = members.Where(m => m.HasPermission(Permissions.KickMembers & Permissions.ManageRoles) && !m.IsBot);
            
            foreach (DiscordMember member in staff)
            {
                var flags = UserFlag.Staff;
                if (member.HasPermission(Permissions.Administrator) || member.IsOwner) flags.Add(UserFlag.EscalatedStaff);

                UserModel? user = guild.Users.FirstOrDefault(u => u.Id == member.Id);
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
                    
                }
            }
        }
    }
}