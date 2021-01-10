using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using Microsoft.Extensions.Logging;
using Silk.Core.Commands.Tests;
using Silk.Core.Database.Models;
using Silk.Core.Services.Interfaces;
using Silk.Core.Utilities;
using Silk.Extensions.DSharpPlus;
using ILogger = Serilog.ILogger;
using Timer = System.Timers.Timer;

namespace Silk.Core.Services
{
    /// <inheritdoc cref="IInfractionService"/>
    public sealed class InfractionService : IInfractionService
    {
        private readonly Timer _timer;
        private readonly ConfigService _configService;
        private readonly IDatabaseService _dbService;
        private readonly ILogger<InfractionService> _logger;
        
        private readonly List<UserInfractionModel> _tempInfractions = new();
        public InfractionService(ConfigService configService, IDatabaseService dbService, ILogger<InfractionService> logger)
        {
            _configService = configService;
            _dbService = dbService;
            _logger = logger;
            _timer = new(TimeSpan.FromSeconds(30).TotalMilliseconds);
            _timer.Elapsed += (_, _) => OnTick();
        }


        public async Task KickAsync(DiscordMember member, DiscordChannel channel, UserInfractionModel infraction)
        {
            await member.RemoveAsync(infraction.Reason);
            GuildConfigModel config = await _configService.GetConfigAsync(member.Guild.Id);
            if (config.GeneralLoggingChannel is 0)
            {
                _logger.LogTrace($"No available log channel for guild! | {member.Guild.Id}");
                await channel.SendMessageAsync($"Kicked **{member.Username}#{member.Discriminator}**!");
            }
            else
            {
                GuildModel guild = (await _dbService.GetGuildAsync(member.Guild.Id))!;
                DiscordEmbedBuilder embed = EmbedHelper
                    .CreateEmbed($"Case #{null} | User {member.Mention}",
                    $"{member.Mention} was kicked from the server by for ```{infraction.Reason}```", DiscordColor.PhthaloGreen);
                embed.WithFooter($"Staff member: <@{infraction.Enforcer}> | {infraction.Enforcer}");

                await channel.SendMessageAsync($":boot: Kicked **{member.Username}#{member.Discriminator}**!");
            }
        }

        public async Task BanAsync(DiscordMember member, DiscordChannel channel, UserInfractionModel infraction)
        {

            
            await member.BanAsync(0, infraction.Reason);
            GuildConfigModel config = await _configService.GetConfigAsync(member.Guild.Id);
            if (config.GeneralLoggingChannel is 0)
            {
                _logger.LogWarning($"No available logging channel for guild! | {member.Guild.Id}");
                await channel.SendMessageAsync($":hammer: Banned **{member.Username}#{member.Discriminator}**!");
            }
            else
            {
                GuildModel guild = (await _dbService.GetGuildAsync(member.Guild.Id))!;
                int infractions = guild.Users.Sum(u => u.Infractions.Count);
                DiscordEmbedBuilder embed = EmbedHelper
                    .CreateEmbed($"Case #{infractions} | User {member.Username}",
                        $"{member.Mention} was banned from the server by for ```{infraction.Reason}```", DiscordColor.IndianRed);
                embed.WithFooter($"Staff member: {infraction.Enforcer}");

                await channel.SendMessageAsync($":hammer: Banned **{member.Username}#{member.Discriminator}**!");
                await channel.Guild.Channels[config.GeneralLoggingChannel].SendMessageAsync(embed: embed);
            }
        }
        public async Task TempBanAsync(DiscordMember member, DiscordChannel channel, UserInfractionModel infraction) { }
        public async Task MuteAsync(DiscordMember member, DiscordChannel channel, UserInfractionModel infraction) { }
        public async Task<UserInfractionModel> CreateInfractionAsync(DiscordMember member, DiscordMember enforcer, InfractionType type, string reason = "Not given.")
        {
            UserModel user = await _dbService.GetOrCreateUserAsync(member.Guild.Id, member.Id);
            UserInfractionModel infraction = new()
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
        public async Task<UserInfractionModel> CreateTemporaryInfractionAsync(DiscordMember member, DiscordMember enforcer, InfractionType type, string reason = "Not given.", DateTime? expiration = null)
        {
            if (type is not InfractionType.SoftBan or InfractionType.Mute) 
                throw new ArgumentException("Is not a temporary infraction type!", nameof(type));
            
            UserInfractionModel infraction = await CreateInfractionAsync(member, enforcer, type, reason);
            infraction.Expiration = expiration;
            return infraction;
        }
        public async Task<bool> ShouldAddInfractionAsync(DiscordMember member) { return false; }
        public void AddTemporaryInfraction(UserInfractionModel infraction)
        {
            throw new NotImplementedException();
        }



        private void OnTick() { }
    }
}