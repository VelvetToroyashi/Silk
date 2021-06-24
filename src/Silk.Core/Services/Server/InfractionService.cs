using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using Humanizer;
using MediatR;
using Microsoft.Extensions.Logging;
using Silk.Core.Data.DTOs;
using Silk.Core.Data.MediatR.Guilds;
using Silk.Core.Data.MediatR.Infractions;
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
	    private readonly ILogger<IInfractionService> _logger;
	    private readonly IMediator _mediator;
	    private readonly DiscordShardedClient _client;

	    private readonly ConfigService _config;
	    private readonly ICacheUpdaterService _updater;
	    
		// Holds all temporary infractions. This could be a seperate hashset like the mutes, but I digress ~Velvet //
		private readonly List<InfractionDTO> _infractions = new();

		// Fast lookup for mutes. Populated on startup. //
		private readonly HashSet<(ulong user, ulong guild)> _mutes = new();
	    
	    public InfractionService(IMediator mediator, DiscordShardedClient client, ConfigService config, ICacheUpdaterService updater, ILogger<IInfractionService> logger)
	    {
		    _mediator = mediator;
		    _client = client;
		    _config = config;
		    _updater = updater;
		    _logger = logger;
	    }

		/* TODO: Make these methods return Task<InfractionResult>
		 Also did I mention how much I *love* Multi-line to-do statements */
		public async Task<InfractionResult> KickAsync(ulong userId, ulong guildId, ulong enforcerId, string reason)
		{
		    var guild = _client.GetShard(guildId).Guilds[guildId];
		    var member = guild.Members[userId];
		    var enforcer = guild.Members[enforcerId];
		    var embed = CreateUserInfractionEmbed(enforcer, guild.Name, InfractionType.Kick, reason);

		    if (member.IsAbove(enforcer))
			    return InfractionResult.FailedGuildHeirarchy;
		    if (!guild.CurrentMember.HasPermission(Permissions.KickMembers))
			    return InfractionResult.FailedSelfPermissions;
			
		    var notified = true;
		    
		    try { await member.SendMessageAsync(embed); }
		    catch (UnauthorizedException) { notified = false; }

		    try { await member.RemoveAsync(reason); }
		    catch (NotFoundException) { return InfractionResult.FailedMemberGuildCache; }
		    catch (UnauthorizedException) { return InfractionResult.FailedSelfPermissions; } /* This shouldn't apply, but. */

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
	    public Task<InfractionDTO> GenerateInfractionAsync(ulong userId, ulong guildId, ulong enforcerId, InfractionType type, string reason, DateTime? expiration)
	    {
		    return _mediator.Send(new CreateInfractionRequest(userId, enforcerId, guildId, reason, type, expiration));
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
		
		/// <summary>
		/// Logs to the designated mod-log channel, if any.
		/// </summary>
		/// <param name="inf">The infraction to log.</param>
		private async Task LogToModChannel(InfractionDTO inf)
		{
		    await EnsureModLogChannelExistsAsync(inf.GuildId);
		    
		    var config = await _config.GetConfigAsync(inf.GuildId);
		    var guild = _client.GetShard(inf.GuildId).Guilds[inf.GuildId];
		    
		    if (config.LoggingChannel is 0)
			    return; /* It couldn't create a mute channel :(*/

		    var user = await _client.ShardClients[0].GetUserAsync(inf.UserId); /* User may not exist on the server anymore. */
		    var enforcer = _client.GetShard(inf.GuildId).Guilds[inf.GuildId].Members[inf.EnforcerId];
		    
		    var builder = new DiscordEmbedBuilder();
		    var infractions = await _mediator.Send(new GetGuildInfractionsRequest(inf.GuildId));
		    
		    builder
			    .WithTitle($"Case #{infractions.Count()}")
			    .WithAuthor($"{user.Username}#{user.Discriminator}", user.GetUrl(), user.AvatarUrl)
			    .WithThumbnail(enforcer.AvatarUrl, 4096, 4096)
			    .WithDescription("A new case has been added to this guild's list of infractions.")
			    .WithColor(DiscordColor.Gold)
			    .AddField("Type:", inf.Type.Humanize(LetterCasing.Title), true)
			    .AddField("Created:", Formatter.Timestamp(inf.CreatedAt - DateTime.UtcNow, TimestampFormat.LongDateTime), true)
			    .AddField("​", "​", true)
			    .AddField("Offender:", $"**{user.ToDiscordName()}**\n(`{user.Id}`)", true)
			    .AddField("Enforcer:", user == _client.CurrentUser ? "[AUTOMOD]" : $"**{enforcer.ToDiscordName()}**\n(`{enforcer.Id}`)", true)
			    .AddField("Reason:", inf.Reason);

		    if (inf.Duration is TimeSpan ts) /* {} (object) pattern is cursed but works. */
			    builder.AddField("Expires:", Formatter.Timestamp(ts));
		    
		    try { await guild.Channels[config.LoggingChannel].SendMessageAsync(builder); }
		    catch (UnauthorizedException) { /* Log something here, and to the backup channel. */ }
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
			    if (!guild.CurrentMember.HasPermission(Permissions.ManageChannels))
				    return; /* We can't create channels. Sad. */
			    try
			    {
				    var overwrites = new DiscordOverwriteBuilder().For(guild.EveryoneRole).Allow(Permissions.None).Deny(Permissions.All);
				    var chn = await guild.CreateChannelAsync("mod-log", ChannelType.Text, (DiscordChannel) guild.Channels.Values.EntityOfType(ChannelType.Category).First(), overwrites: new[] { overwrites});
				    await _mediator.Send(new UpdateGuildConfigRequest(guildId) {LoggingChannel = chn.Id});
				    _updater.UpdateGuild(guildId);
				    _logger.LogTrace("Updated guild");
			    }
			    catch { /* Igonre. We can't do anything about it :( */}
		    }
	    }
    }
}