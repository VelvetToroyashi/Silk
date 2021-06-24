using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using MediatR;
using Silk.Core.Data.DTOs;
using Silk.Core.Data.Models;
using Silk.Core.Services.Data;
using Silk.Core.Services.Interfaces;
using Silk.Core.Types;
using Silk.Extensions.DSharpPlus;

namespace Silk.Core.Services.Server
{
    public sealed class InfractionService : IInfractionService
    {
	    private readonly IMediator _mediator;
	    private readonly ConfigService _config;
	    private readonly DiscordShardedClient _client;
	    
		// Holds all temporary infractions. This could be a seperate hashset like the mutes, but I digress ~Velvet //
		private readonly List<InfractionDTO> _infractions = new();

		// Fast lookup for mutes. Populated on startup. //
		private readonly HashSet<(ulong user, ulong guild)> _mutes = new();
	    
	    public InfractionService(IMediator mediator, DiscordShardedClient client, ConfigService config)
	    {
		    _mediator = mediator;
		    _client = client;
		    _config = config;
	    }

		/* TODO: Make these methods return Task<InfractionResult>
		 Also did I mention how much I *love* Multi-line to-do statements */
		public async Task<InfractionResult> KickAsync(ulong userId, ulong guildId, ulong enforcerId, string reason)
		{
		    var guild = _client.GetShard(guildId).Guilds[guildId];
		    var member = guild.Members[userId];
		    var enforcer = guild.Members[enforcerId];
		    var embed = CreateUserInfractionEmbed(enforcer, guild.Name, InfractionType.Kick, reason);
		    
		    var notified = true;
		    
		    try { await member.SendMessageAsync(embed); }
		    catch (UnauthorizedException) { notified = false; }

		    try { await member.RemoveAsync(reason); }
		    catch (NotFoundException) { return InfractionResult.FailedMemberGuildCache; }
		    catch (UnauthorizedException) { return InfractionResult.FailedSelfPermissions; }

		    var inf = await GenerateInfractionAsync(userId, guildId, enforcerId, InfractionType.Kick, reason, null);

		    await LogToModChannel(inf);
		    return notified ? InfractionResult.SucceededWithNotification : InfractionResult.SucceededWithoutNotification; 
		}
	    
	    public async Task BanAsync(ulong userId, ulong guildId, ulong enforcerId, string reason, DateTime? expiration = null) { }
	    public async Task StrikeAsync(ulong userId, ulong guildId, ulong enforcerId, string reason, bool isAutoMod = false) { }
	    public async ValueTask<bool> IsMutedAsync(ulong userId, ulong guildId)
	    {
		    return false;
	    }
	    public async Task<InfractionResult> MuteAsync(ulong userId, ulong guildId, ulong enforcerId, string reason, DateTime? expiration)
	    {
		    return InfractionResult.SucceededWithNotification;
	    }
	    public async Task<InfractionStep> GetCurrentInfractionStepAsync(ulong guildId, IEnumerable<InfractionDTO> infractions)
	    {
		    return null;
	    }
	    public async Task<InfractionDTO> GenerateInfractionAsync(ulong userId, ulong guildId, ulong enforcerId, InfractionType type, string reason, DateTime? expiration)
	    {
		    return null;
	    }

	    private static DiscordEmbedBuilder CreateUserInfractionEmbed(DiscordUser enforcer, string guildName, InfractionType type, string reason, DateTime? expiration = default)
	    {
		    var action = type switch
		    {
			    InfractionType.Kick			=> $"You've been kicked from {guildName}!",
			    InfractionType.Ban			=> $"You've been permenantly banned from {guildName}!",
			    InfractionType.SoftBan		=> $"You've been temporarily banned from {guildName}!",
			    InfractionType.Mute			=> $"You've been muted on {guildName}!",
			    InfractionType.AutoModMute	=> $"You've been automatically muted on {guildName}!",
			    InfractionType.Strike		=> $"You've been warned on {guildName}!"
		    };
		    
		    var embed = new DiscordEmbedBuilder()
				    .WithTitle(action)
					.WithAuthor($"{enforcer.Username}#{enforcer.Discriminator}", enforcer.GetUrl(), enforcer.AvatarUrl)
				    .AddField("Reason:", reason)
				    .AddField("Infraction occured:", 
					    $"{Formatter.Timestamp(TimeSpan.Zero, TimestampFormat.LongDateTime)}\n\n({Formatter.Timestamp(TimeSpan.Zero)})")
				    .AddField("Enforcer:", enforcer.Id.ToString());

		    if (expiration.HasValue)
			    embed.AddField("Expires:", Formatter.Timestamp(expiration.Value - DateTime.UtcNow));

		    return embed;
	    }

	    private async Task LogToModChannel(InfractionDTO inf)
	    {

	    }
	    
	    /// <summary>
	    /// Ensures a moderation channel exists. If it doesn't one will be created, and hidden.
	    /// </summary>
	    private async Task EnsureModLogChannelExistsAsync(ulong guildId)
	    {
		    GuildConfig config = await _config.GetConfigAsync(guildId);
		    DiscordGuild guild = _client.GetShard(guildId).Guilds[guildId];
		    if (config.Id is 0)
		    {

		    }
	    }
    }
}