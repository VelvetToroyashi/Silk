using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using MediatR;
using Silk.Core.Data.DTOs;
using Silk.Core.Data.Models;
using Silk.Core.Services.Interfaces;
using Silk.Core.Types;
using Silk.Extensions.DSharpPlus;

namespace Silk.Core.Services.Server
{
    public sealed class InfractionService : IInfractionService
    {
	    private readonly IMediator _mediator;
	    private readonly DiscordShardedClient _client;
	    private readonly List<InfractionDTO> _infractions = new();
	    private readonly HashSet<(ulong user, ulong guild)> _mutes = new();
	    
	    public InfractionService(IMediator mediator, DiscordShardedClient client)
	    {
		    _mediator = mediator;
		    _client = client;
	    }

		/* TODO: Make these methods return Task<InfractionResult>
		 Also did I mention how much I *love* Multi-line to-do statements */
	    public async Task KickAsync(ulong userId, ulong guildId, ulong enforcerId, string reason)
	    {
		    var guild = _client.GetShard(guildId).Guilds[guildId];
		    var member = guild.Members[userId];
		    var enforcer = guild.Members[enforcerId];
		    var embed = CreateUserInfractionEmbed(enforcer, guild.Name, InfractionType.Kick, reason);
		    try
		    {
			    await member.SendMessageAsync(embed);
			    await member.RemoveAsync(reason);
		    }
		    catch (NotFoundException) { }
		    catch (UnauthorizedException) { }
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
	    public async Task<InfractionStep> GetCurrentInfractionStepAsync(ulong guildId, int infractions)
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
    }
}