using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Timers;
using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using Silk.Core.Database.Models;
using Silk.Core.Exceptions;
using Silk.Core.Services.Interfaces;
using Silk.Core.Utilities;
using Silk.Extensions;

namespace Silk.Core.Services
{
    /// <inheritdoc cref="IInfractionService"/>
    public sealed class InfractionService : IInfractionService
    {
        private readonly Timer _timer;
        private readonly ConfigService _configService;
        private readonly IDatabaseService _dbService;
        private readonly ILogger<InfractionService> _logger;
        private readonly DiscordShardedClient _client;

        private readonly List<Infraction> _tempInfractions = new();
        public InfractionService(ConfigService configService, IDatabaseService dbService, ILogger<InfractionService> logger, DiscordShardedClient client)
        {
            _configService = configService;
            _dbService = dbService;
            _logger = logger;
            _client = client;
            _timer = new(TimeSpan.FromSeconds(30).TotalMilliseconds);
            _timer.Elapsed += async (_, _) => await OnTick();
            _timer.Start();

            LoadInfractions();
        }


        public async Task SilentKickAsync(DiscordMember member, DiscordChannel channel, Infraction infraction) { throw new NotImplementedException(); }

        public async Task VerboseKickAsync(DiscordMember member, DiscordChannel channel, Infraction infraction, DiscordEmbed embed)
        {
            // Validation is handled by the command class. //
            _ = member.RemoveAsync(infraction.Reason);
            GuildConfig config = await _configService.GetConfigAsync(member.Guild.Id);

            if (config.GeneralLoggingChannel is 0)
                _logger.LogTrace($"No available log channel for guild! | {member.Guild.Id}");
            else
                _ = channel.Guild.Channels[config.GeneralLoggingChannel].SendMessageAsync(embed);

            _ = channel.SendMessageAsync($":boot: Kicked **{member.Username}#{member.Discriminator}**!");
        }

        public async Task BanAsync(DiscordMember member, DiscordChannel channel, Infraction infraction)
        {
            await member.BanAsync(0, infraction.Reason);
            GuildConfig config = await _configService.GetConfigAsync(member.Guild.Id);
            await ApplyInfractionAsync(infraction.User, infraction);
            if (config.GeneralLoggingChannel is 0)
            {
                _logger.LogWarning($"No available logging channel for guild! | {member.Guild.Id}");
                await channel.SendMessageAsync($":hammer: Banned **{member.Username}#{member.Discriminator}**!");
            }
            else
            {
                Guild guild = (await _dbService.GetGuildAsync(member.Guild.Id))!;
                int infractions = guild.Users.Sum(u => u.Infractions.Count);
                
                DiscordEmbedBuilder embed = EmbedHelper.CreateEmbed($"Case #{infractions} | User {member.Username}",
                    $"{member.Mention} was banned from the server by for ```{infraction.Reason}```", DiscordColor.IndianRed)
                    .WithFooter($"Staff member: {infraction.Enforcer}");

                await channel.SendMessageAsync($":hammer: Banned **{member.Username}#{member.Discriminator}**!");
                await channel.Guild.Channels[config.GeneralLoggingChannel].SendMessageAsync(embed);
            } 
        }
        public async Task TempBanAsync(DiscordMember member, DiscordChannel channel, Infraction infraction)
        {
            
        }
        public async Task MuteAsync(DiscordMember member, DiscordChannel channel, Infraction infraction)
        {
            GuildConfig config = await _configService.GetConfigAsync(infraction.User.Guild.Id);
            
            if (!channel.Guild.Roles.TryGetValue(config.MuteRoleId, out DiscordRole? muteRole))
            {
                await channel.SendMessageAsync("Mute role doesn't exist on server!");
                return;
            }
            
            await member.GrantRoleAsync(muteRole);
            
            User user = await _dbService.GetOrCreateGuildUserAsync(member.Guild.Id, member.Id);
            await ApplyInfractionAsync(user, infraction);
            
            _tempInfractions.Add(infraction);
            _logger.LogTrace($"Added temporary infraction to {member.Id}!");
        }
        public async Task<Infraction> CreateInfractionAsync(DiscordMember member, DiscordMember enforcer, InfractionType type, string reason = "Not given.")
        {
            User user = await _dbService.GetOrCreateGuildUserAsync(member.Guild.Id, member.Id);
            Infraction infraction = new()
            {
                Enforcer = enforcer.Id,
                Reason = reason,
                InfractionTime = DateTime.Now,
                UserId = member.Id,
                InfractionType = type,
            };
            user.Infractions.Add(infraction);
            // We will handle having automatic infractions later //
            // This will have a method to accompany it soon:tm: //
            await _dbService.UpdateGuildUserAsync(user);
            return infraction;
        }
        public async Task<Infraction> CreateTemporaryInfractionAsync(DiscordMember member, DiscordMember enforcer, InfractionType type, string reason = "Not given.", DateTime? expiration = null)
        {
            if (type is not (InfractionType.SoftBan or InfractionType.Mute))
                throw new ArgumentException("Is not a temporary infraction type!", nameof(type));

            Infraction infraction = await CreateInfractionAsync(member, enforcer, type, reason);
            infraction.Expiration = expiration;
            return infraction;
        }

        public async Task<bool> ShouldAddInfractionAsync(DiscordMember member)
        {
            User? user = await _dbService.GetGuildUserAsync(member.Guild.Id, member.Id);
            return !user?.Flags.HasFlag(UserFlag.Staff) ?? true;
        }
        public async Task<bool> HasActiveMuteAsync(DiscordMember member) => false;

        private async Task ProcessSoftBanAsync(DiscordGuild guild, GuildConfig config, Infraction inf)
        {
            await guild.UnbanMemberAsync(inf.UserId);
            _logger.LogTrace($"Unbanned {inf.UserId} | SoftBan expired.");
            if (config.GeneralLoggingChannel is 0)
            {
                _logger.LogTrace($"Logging channel not configured for {guild.Id}.");
                return;
            }

            DiscordEmbed embed = EmbedHelper.CreateEmbed("Ban expired!", $"<@{inf.UserId}>'s ban has expired.", DiscordColor.Goldenrod);
            await guild.Channels[config.GeneralLoggingChannel].SendMessageAsync(embed);
        }

        private async Task ProcessTempMuteAsync(DiscordGuild guild, GuildConfig config, Infraction inf)
        {
            if (!guild.Members.TryGetValue(inf.UserId, out DiscordMember? mutedMember))
            {
                _logger.LogTrace("Cannot unmute member outside of guild!");
            }
            else
            {
                await mutedMember.RevokeRoleAsync(guild.Roles[config.MuteRoleId], "Temporary mute expired.");
                _logger.LogTrace($"Unmuted {inf.UserId} | TempMute expired.");
            }
        }

        private async Task ApplyInfractionAsync(User user, Infraction infraction)
        {
            user.Infractions.Add(infraction);
            await _dbService.UpdateGuildUserAsync(user);
        }
        
        private async Task OnTick()
        {
            if (_tempInfractions.Count is 0) return;
            IEnumerable<IGrouping<ulong, Infraction>> infractions = _tempInfractions
                .Where(i => ((DateTime) i.Expiration!).Subtract(DateTime.Now).Seconds < 0)
                .GroupBy(x => x.User.Guild.Id);
            foreach (var inf in infractions)
            {
                _logger.LogTrace($"Processing infraction in guild {inf.Key}");
                DiscordGuild guild = _client.ShardClients.Values.SelectMany(s => s.Guilds.Values).First(g => g.Id == inf.Key);
                GuildConfig config = await _configService.GetConfigAsync(guild.Id);
                foreach (Infraction infraction in inf)
                {
                    _logger.LogTrace($"Infraction {infraction.Id} | User {infraction.UserId}");
                    Task task = infraction.InfractionType switch
                    {
                        InfractionType.SoftBan => ProcessSoftBanAsync(guild, config, infraction),
                        InfractionType.AutoModMute or InfractionType.Mute => ProcessTempMuteAsync(guild, config, infraction),
                        _ => throw new ArgumentTypeException("Type is not temporary infraction!")
                    };
                    await task;
                    _tempInfractions.Remove(infraction);
                }
            }
        }

        private void LoadInfractions()
        {
            _logger.LogTrace("Building InfractionService cache");

            IEnumerable<Infraction> infractions = _dbService.GetActiveInfractionsAsync().GetAwaiter().GetResult();
            
            _tempInfractions.AddRange(infractions);
            _logger.LogTrace("Loaded all applicable infractions!");
        }
        
        private static void ValidateInfraction(Infraction infraction)
        {
            if (infraction.Enforcer is 0) throw new ArgumentNullException();
            if (infraction.Reason is "") throw new ArgumentNullException();
            if (infraction.User is null) throw new ArgumentNullException();
        }

        public Task ProgressInfractionStepAsync(DiscordMember member, Infraction infraction)
        {
            throw new NotImplementedException();
        }
    }
}