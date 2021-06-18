using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;
using Silk.Core.Data.DTOs;
using Silk.Core.Data.MediatR.Infractions.cs;
using Silk.Core.Data.MediatR.Users;
using Silk.Core.Data.Models;
using Silk.Core.Services.Data;
using Silk.Core.Services.Interfaces;
using Silk.Extensions.DSharpPlus;

namespace Silk.Core.Services.Server
{
	public class InfractionService : IInfractionService
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
		public async Task KickAsync(ulong userId, ulong guildId, string reason) { }
		public async Task BanAsync(ulong userId, ulong guildId, string reason, DateTime? expiration = null) { }
		public async Task StrikeAsync(ulong userId, ulong guildId, string reason, bool isAutoMod = false) { }
		public ValueTask<bool> IsMutedAsync(ulong userId, ulong guildId)
		{
			var inf = _infractions.Find(i => 
											 i.UserId == userId && 
			                                 i.GuildId == guildId &&
			                                 i.Type is InfractionType.Mute);
			return ValueTask.FromResult(inf is not null);
		}
		public async Task<bool> MuteAsync(ulong userId, ulong guildId, ulong enforcerId, TimeSpan? duration, string reason)
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
				var infraction = await GenerateInfractionAsync(userId, enforcerId, guildId, InfractionType.Mute, reason, duration.HasValue ? DateTime.UtcNow + duration : null);
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
			var index = Math.Min(conf.InfractionSteps.Count - 1, infractions);
			return conf.InfractionSteps[index];
		}

		private Task EnsureUserExistsAsync(ulong userId, ulong guildId)
			=> _mediator.Send(new GetOrCreateUserRequest(guildId, userId));

		private async Task<bool> SendDMLogAsync(ulong userId, DiscordEmbed embed)
		{
			var member = _client.GetMember(m => m.Id == userId);
			if (member is null)
			{
				_logger.LogWarning("Attempted to DM user that does not exist anymore");
				return false;
			}
			else
			{
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
}