using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;
using Silk.Core.Data.DTOs;
using Silk.Core.Data.MediatR.Guilds;
using Silk.Core.Data.MediatR.Infractions;
using Silk.Core.Data.MediatR.Users;
using Silk.Core.Data.Models;
using Silk.Core.Services.Data;
using Silk.Core.Services.Interfaces;
using Silk.Core.Types;
using Silk.Extensions;
using Silk.Extensions.DSharpPlus;

namespace Silk.Core.Services.Server
{
    public sealed class InfractionService : IInfractionService
    {
        private readonly IMediator _mediator;
        private readonly ILogger<InfractionService> _logger;
        private readonly IServiceCacheUpdaterService _updater;
        
        private readonly List<InfractionDTO> _infractions = new();
        private readonly DiscordShardedClient _client;
        private readonly ConfigService _config;

        public InfractionService(IMediator mediator, 
            DiscordShardedClient client, 
            ILogger<InfractionService> logger,
            ConfigService config, 
            IServiceCacheUpdaterService updater)
        {
            _mediator = mediator;
            _client = client;
            _logger = logger;
            _config = config;
            _updater = updater;
        }

        public async Task KickAsync(ulong userId, ulong guildId, ulong enforcerId, string reason) {}

        public async Task BanAsync(ulong userId, ulong guildId, ulong enforcerId, string reason, DateTime? expiration = null){}

        public async Task StrikeAsync(ulong userId, ulong guildId, ulong enforcerId, string reason, bool isAutoMod = false)
        {
            var user = await _mediator.Send(new GetOrCreateUserRequest(guildId, userId, UserFlag.WarnedPrior));
            user.Flags |= UserFlag.WarnedPrior;

            InfractionDTO infraction;
            
            var config = await _config.GetConfigAsync(guildId);
            if (!config.AutoEscalateInfractions && !isAutoMod)
            {
                infraction = await GenerateInfractionAsync(userId, enforcerId, guildId, InfractionType.Strike, reason, null);
                await _mediator.Send(new UpdateUserRequest(guildId, userId, user.Flags));
            }
            else
            {
                var userInfractions = await _mediator.Send(new GetUserInfractionsRequest(guildId, userId));

                InfractionStep? infractionLevel = null;

                if (config.InfractionSteps.Any())
                    infractionLevel = await GetCurrentInfractionStepAsync(guildId, userInfractions.Count() + 1);

                var action = infractionLevel?.Type ?? InfractionType.Strike;
                infraction = await GenerateInfractionAsync(userId, enforcerId, guildId, action, reason,
                    infractionLevel?.Duration == default ? null : DateTime.UtcNow + infractionLevel.Duration.Time);
            }

            Func<ulong, ulong, ulong, string, DateTime?, Task> t = infraction.Type switch
            {
                InfractionType.Ban or InfractionType.SoftBan => BanAsync,
                InfractionType.Mute or InfractionType.AutoModMute => MuteAsync,
                InfractionType.Strike => LogStrikeAsync,
                InfractionType.Kick => (u, g, e, r, _) => KickAsync(u, g, e, r),
                _ => throw new ArgumentException("I don't know. I am just wah.")
            };

            await t(userId, guildId, enforcerId, reason, DateTime.UtcNow + infraction.Duration);

            async Task LogStrikeAsync(ulong userId, ulong enforcerId, ulong guildId, string reason, DateTime? expiration)
            {
                if (config.LoggingChannel is 0)
                    return;

                var guild = _client.GetShard(guildId).Guilds[guildId];
                var exists = guild.Channels.TryGetValue(config.LoggingChannel, out var chn);

                if (!exists)
                {
                    _logger.LogWarning("Channel exists in config but is not present on guild!");
                    return;
                }

                if (!CanLogToLogChannel())
                {
                    _logger.LogWarning("Cannot log strike to channel due to insufficient permissions!");
                    return;
                }


                var enforcer = await guild.GetMemberAsync(enforcerId);
                DiscordMember victim = await guild.GetMemberAsync(userId);

                var cases = await _mediator.Send(new GetGuildInfractionsRequest(guildId));
                TimeSpan infractionOccured = DateTime.UtcNow - infraction.CreatedAt;
                var embed = new DiscordEmbedBuilder()
                    .WithTitle($"Strike : Case #{cases.Count()}")
                    .WithAuthor(victim.Username, victim.GetUrl(), victim.GuildAvatarUrl)
                    .WithThumbnail(enforcer.GuildAvatarUrl, 4096, 4096)
                    .AddField("Staff member:", $"{enforcer.Username}#{enforcer.Discriminator}\n({enforcer.Id})", true)
                    .AddField("Applied to:", $"{victim.Username}#{victim.Discriminator}\n({victim.Id})", true)
                    .AddField("Infraction occured:", $"{Formatter.Timestamp(infractionOccured, TimestampFormat.LongDateTime)} ({Formatter.Timestamp(infractionOccured)})")
                    .AddField("Reason:", reason);

                await chn.SendMessageAsync(embed);

                bool CanLogToLogChannel() =>
                    (chn!.PermissionsFor(guild!.CurrentMember) &
                     (Permissions.SendMessages | Permissions.EmbedLinks)) is not 0;
            }
        }

        private async Task<DiscordEmbedBuilder?> GenerateNotificationEmbedAsync(InfractionDTO infraction)
        {
            ulong guildId = infraction.GuildId;
            
            var guild = _client.GetShard(guildId).Guilds[guildId];
            var enforcer = await guild.GetMemberAsync(infraction.EnforcerId);

            var embed = new DiscordEmbedBuilder();
            embed.WithAuthor(enforcer.Username, enforcer.GetUrl(), enforcer.AvatarUrl);

            var title = infraction.Type switch
            {
                InfractionType.Strike => $"You've received a strike in {guild.Name}!",
                InfractionType.Mute => $"You've received a mute in {guild.Name}!",
                InfractionType.SoftBan => $"You've been temporarily banned from {guild.Name}!",
                InfractionType.Ban => $"You've been permanently banned from {guild.Name}!",
                InfractionType.Kick => $"You've been kicked from {guild.Name}!",
                InfractionType.AutoModMute => throw new InvalidOperationException("How did you even manage to do this?"),
                InfractionType.Ignore => null, /* We shouldn't be logging to the user. */
                _ => throw new ArgumentException($"Unknown enum value: {infraction.Type}")
            };

            embed.WithTitle(title)
                .WithDescription($"Reason: {infraction.Reason}");

            if (infraction.Duration is not null)
                embed.AddField("Expires:", Formatter.Timestamp(infraction.Duration.Value));
            
            return embed;
        }

        public ValueTask<bool> IsMutedAsync(ulong userId, ulong guildId)
        {
            var inf = _infractions.Find(i =>
                i.UserId == userId &&
                i.GuildId == guildId &&
                i.Type is InfractionType.Mute or InfractionType.AutoModMute);
            
            return ValueTask.FromResult(inf is not null);
        }

        public async Task<MuteResult> MuteAsync(ulong userId, ulong guildId, ulong enforcerId, string reason, DateTime? expiration)
        {
            var user = await _mediator.Send(new GetOrCreateUserRequest(guildId, userId));

            var shard = _client.GetShard(guildId);
            var guild = shard.Guilds[guildId];
            
            if (await IsMutedAsync(userId, guildId))
            {
                var currentInfraction = GetTemporaryInfractionForUser(userId, guildId, InfractionType.Mute) ??
                                        GetTemporaryInfractionForUser(userId, guildId, InfractionType.AutoModMute);

                await _mediator.Send(new UpdateInfractionRequest(currentInfraction!.Id, expiration));
            }

            try
            {
                if (!await EnsureMuteRoleExistsAsync(guild))
                    return MuteResult.CouldNotCreateMuteRole;

                var conf = await _config.GetConfigAsync(guildId);
                var role = guild.GetRole(conf.MuteRoleId);

                await guild.Members[userId].GrantRoleAsync(role, user.Flags.HasFlag(UserFlag.ActivelyMuted) ? "Re-applying mute." : reason);
                
                _logger.LogTrace("Successfully muted member");
            }
            catch (KeyNotFoundException)
            {
                _logger.LogWarning("Member left guild whilst applying mute. Reapplying on re-join");
                return MuteResult.MemberLeftBeforeMute;
            }
            catch (UnauthorizedException)
            {
                _logger.LogTrace("Role grant denied by guild hierarchy");
                return MuteResult.CouldNotApplyMuteRole;
            }

            user.Flags |= UserFlag.ActivelyMuted;
            await _mediator.Send(new UpdateUserRequest(guildId, userId, user.Flags));
            
            var infraction = await GenerateInfractionAsync(userId, enforcerId, guildId,
                enforcerId == _client.CurrentUser.Id ? InfractionType.AutoModMute : InfractionType.Mute, reason,
                expiration);

            _infractions.Add(infraction);

            var notified = await NotifyUserAsync(userId, await GenerateNotificationEmbedAsync(infraction));

            return notified ? MuteResult.SucceededWithNotification : MuteResult.SucceededWithoutNotification;
        }

        public Task<InfractionDTO> GenerateInfractionAsync(ulong userId, ulong enforcerId, ulong guildId,
            InfractionType type, string reason, DateTime? expiration, bool holdAgainstUser = true)
            => _mediator.Send(new CreateInfractionRequest(userId, enforcerId, guildId, reason, type, expiration,
                holdAgainstUser));

        public async Task<InfractionStep> GetCurrentInfractionStepAsync(ulong guildId, int infractions)
        {
            // This is primarily used by AutoMod //
            var conf = await _config.GetConfigAsync(guildId);
            var index = Math.Max(0, Math.Min(conf.InfractionSteps.Count - 1, infractions));
            return conf.InfractionSteps[index];
        }

        private Task EnsureUserExistsAsync(ulong userId, ulong guildId)
            => _mediator.Send(new GetOrCreateUserRequest(guildId, userId));


        private InfractionDTO? GetTemporaryInfractionForUser(ulong userId, ulong guild, InfractionType type)
            => _infractions.Find(i => i.GuildId == guild && i.UserId == userId && i.Type == type);

        private async Task LogToLogChannel(ulong userId, ulong enforcerId, ulong guildId, InfractionDTO infraction)
        {
            var config = await _config.GetConfigAsync(guildId);

            if (config.LoggingChannel is 0)
                return;

            var guild = _client.GetShard(guildId).Guilds[guildId];
            var exists = guild.Channels.TryGetValue(config.LoggingChannel, out var chn);

            if (!exists)
            {
                _logger.LogWarning("Channel exists in config but is not present on guild!");
                return;
            }

            if (!CanLogToLogChannel())
            {
                _logger.LogWarning("Cannot log strike to channel due to insufficient permissions!");
                return;
            }

            DiscordMember enforcer = await guild.GetMemberAsync(enforcerId);
            DiscordMember victim = await guild.GetMemberAsync(userId);

            var cases = await _mediator.Send(new GetGuildInfractionsRequest(guildId));
            TimeSpan infractionOccured = DateTime.UtcNow - infraction.CreatedAt;
            var embed = new DiscordEmbedBuilder()
                .WithTitle($"Infraction : Case #{cases.Count()}")
                .WithAuthor(victim.Username, victim.GetUrl(), victim.GuildAvatarUrl)
                .WithThumbnail(enforcer.GuildAvatarUrl, 4096, 4096)
                .AddField("Staff member:", $"{enforcer.Username}#{enforcer.Discriminator}\n({enforcer.Id})", true)
                .AddField("Applied to:", $"{victim.Username}#{victim.Discriminator}\n({victim.Id})", true)
                .AddField("Infraction occured:", $"{Formatter.Timestamp(infractionOccured, TimestampFormat.LongDateTime)} ({Formatter.Timestamp(infractionOccured)})", true)
                .AddField("Reason:", infraction.Reason, true)
                .AddField("Type:", infraction.Type.ToString(), true);

            await chn!.SendMessageAsync(embed);

            bool CanLogToLogChannel() =>
                (chn!.PermissionsFor(guild!.CurrentMember) &
                 (Permissions.SendMessages | Permissions.EmbedLinks)) is not 0;
        }

        private async Task<bool> EnsureMuteRoleExistsAsync(DiscordGuild guild)
        {
            var config = await _config.GetConfigAsync(guild.Id);

            if (RoleExistsAndIsRestrictive(guild, config.MuteRoleId))
                return true;

            // Attempt to generate the role and update the guild config. //
            if (!guild.CurrentMember.HasPermission(Permissions.ManageRoles))
            {
                _logger.LogWarning("Could not generate mute role for guild | Insufficient permissions");
                return false;
            }

            var role = await guild.CreateRoleAsync("Muted",
                Permissions.None | Permissions.AccessChannels,
                DiscordColor.Gray, false, false,
                "Mute role was not present on guild.");
            
            await role.ModifyPositionAsync(guild.CurrentMember.Hierarchy - 1); // Set it below the bot, and attempt to apply //
            await _mediator.Send(new UpdateGuildConfigRequest(guild.Id) {MuteRoleId = role.Id});
            _updater.UpdateGuild(guild.Id);
            
            return true;

            static bool RoleExistsAndIsRestrictive(DiscordGuild guild, ulong roleId)
                => guild.Roles.TryGetValue(roleId, out var role) && !role.HasPermission(Permissions.SendMessages);
        }


        private async Task<bool> NotifyUserAsync(ulong userId, DiscordEmbed embed)
        {
            var member = _client.GetMember(m => m.Id == userId);
            if (member is null)
            {
                _logger.LogWarning("Attempted to DM user that does not exist anymore");
                return false;
            }

            try
            {
                await member.SendMessageAsync(embed);
                _logger.LogTrace("Successfully dispatched message to user");
                return true;
            }
            catch
            {
                _logger.LogWarning("Could not DM user");
                return false;
            }
        }
    }
}