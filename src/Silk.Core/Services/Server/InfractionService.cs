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
using Silk.Core.Data.MediatR.Infractions;
using Silk.Core.Data.MediatR.Users;
using Silk.Core.Data.Models;
using Silk.Core.Services.Data;
using Silk.Core.Services.Interfaces;
using Silk.Extensions.DSharpPlus;

namespace Silk.Core.Services.Server
{
	public sealed class InfractionService : IInfractionService
	{
		private readonly List<InfractionDTO> _infractions = new();
		private readonly IMediator _mediator;
		private readonly DiscordShardedClient _client;
		private readonly ILogger<InfractionService> _logger;
		private readonly ConfigService _config;
		
		public InfractionService(IMediator mediator, DiscordShardedClient client, ILogger<InfractionService> logger, ConfigService config)
		{
			_mediator = mediator;
			_client = client;
			_logger = logger;
			_config = config;
		}
		public async Task KickAsync(ulong userId, ulong guildId, ulong enforcerId,  string reason) { }
		public async Task BanAsync(ulong userId, ulong guildId, ulong enforcerId, string reason, DateTime? expiration = null) { }
		public async Task StrikeAsync(ulong userId, ulong guildId, ulong enforcerId, string reason, bool isAutoMod = false)
		{
			var user = await _mediator.Send(new GetOrCreateUserRequest(guildId, userId, UserFlag.WarnedPrior));
			user.Flags |= UserFlag.WarnedPrior;
			
			InfractionDTO infraction;
			var config = await _config.GetConfigAsync(guildId);
			if (!config.AutoEscalateInfractions && !isAutoMod)
			{
				infraction = await GenerateInfractionAsync(userId, enforcerId, guildId, InfractionType.Strike, reason, null);
			}
			else
			{
				var userInfractions = await _mediator.Send(new GetUserInfractionsRequest(guildId, userId));
				
				InfractionStep? infractionLevel = null;
				
				if (config.InfractionSteps.Any())
					infractionLevel = await GetCurrentInfractionStepAsync(guildId, userInfractions.Count());

				var action = infractionLevel?.Type ?? InfractionType.Strike;
				infraction = await GenerateInfractionAsync(userId, enforcerId, guildId, action, reason, infractionLevel?.Expiration == default ? null : DateTime.UtcNow + infractionLevel.Expiration.Time);
			}
			
			var guild = _client.GetShard(guildId).Guilds[guildId];
			var enforcer = await guild.GetMemberAsync(infraction.EnforcerId);
			
			var embed = new DiscordEmbedBuilder();
			embed.WithAuthor(enforcer.Username, enforcer.GetUrl(), enforcer.AvatarUrl);

			var title = infraction.Type switch
			{
				InfractionType.Strike => $"You've received a strike in {guild.Name}!",
				InfractionType.Mute => $"You've received a mute in {guild.Name}!",
				InfractionType.SoftBan => $"You've been temporarily banned from {guild.Name}!",
				InfractionType.Ban => $"You've been permenantly banned from {guild.Name}!",
				InfractionType.Kick => $"You've been kicked from {guild.Name}!",
				InfractionType.AutoModMute => throw new InvalidOperationException("How did you even manage to do this?"),
				InfractionType.Ignore => null, /* We shouldn't be logging to the user. */
				_ => throw new ArgumentException($"Unknown enum value: {infraction.Type}")
			};

			embed.WithTitle(title)
				.WithDescription($"Reason: {infraction.Reason}");

			if (infraction.Duration is not null)
				embed.AddField("Expires:", Formatter.Timestamp(infraction.Duration.Value));
			
			await NotifyUserAsync(userId, embed);

			Func<ulong, ulong, ulong, string, DateTime?, Task> t = infraction.Type switch
			{
				InfractionType.Ban  or InfractionType.SoftBan => BanAsync,
				InfractionType.Mute or InfractionType.AutoModMute => MuteAsync,
				InfractionType.Strike => LogToLogChannel,
				InfractionType.Kick => (u, g, e, r, _) => KickAsync(u, g, e, r),
				_ => throw new ArgumentException("I don't know. I am just wah.")
			};

			await t(userId, guildId, enforcerId, reason, DateTime.UtcNow + infraction.Duration);
		}
		private async Task LogToLogChannel(ulong arg1, ulong arg2, ulong arg3, string arg4, DateTime? arg5)
		{
		}

		public ValueTask<bool> IsMutedAsync(ulong userId, ulong guildId)
		{
			var inf = _infractions.Find(i => 
											 i.UserId == userId && 
			                                 i.GuildId == guildId &&
			                                 i.Type is InfractionType.Mute);
			return ValueTask.FromResult(inf is not null);
		}
		
		public async Task<bool> MuteAsync(ulong userId, ulong guildId, ulong enforcerId, string reason, DateTime? expiration)
		{
			var user = await _mediator.Send(new GetOrCreateUserRequest(guildId, userId));
			
			var shard = _client.GetShard(guildId);
			var guild = shard.Guilds[guildId];
			var conf = await _config.GetConfigAsync(guildId);
			
			try
			{
				var role = guild.GetRole(conf.MuteRoleId);
				await guild.Members[userId].GrantRoleAsync(role, user.Flags.HasFlag(UserFlag.ActivelyMuted) ? "Re-applying mute." : reason);
				_logger.LogTrace("Successfully muted member");
			}
			catch (KeyNotFoundException)
			{
				_logger.LogWarning("Member left guild whilst applying mute. Reapplying on re-join");
				return false;
			}
			catch (UnauthorizedException)
			{
				_logger.LogTrace("Role grant denied by guild heirarchy");
				return false;
			}
			finally
			{
				user.Flags |= UserFlag.ActivelyMuted;
				await _mediator.Send(new UpdateUserRequest(guildId, userId, user.Flags));
				var infraction = await GenerateInfractionAsync(userId, enforcerId, guildId, InfractionType.Mute, reason, expiration);
				
				_infractions.Add(infraction);
			}
			return true;
		}
		
		public Task<InfractionDTO> GenerateInfractionAsync(ulong userId, ulong enforcerId, ulong guildId, InfractionType type, string reason, DateTime? expiration, bool holdAgainstUser = true)
			=> _mediator.Send(new CreateInfractionRequest(userId, enforcerId, guildId, reason, type, expiration, holdAgainstUser));
		
		public async Task<InfractionStep> GetCurrentInfractionStepAsync(ulong guildId, int infractions)
		{
			// This is primarily used by AutoMod //
			var conf = await _config.GetConfigAsync(guildId);
			var index = Math.Max(0, Math.Min(conf.InfractionSteps.Count - 1, infractions));
			return conf.InfractionSteps[index];
		}

		private Task EnsureUserExistsAsync(ulong userId, ulong guildId)
			=> _mediator.Send(new GetOrCreateUserRequest(guildId, userId));

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
				_logger.LogTrace("Succesfully dispatched message to user");
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