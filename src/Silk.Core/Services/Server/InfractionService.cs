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

		public async Task<InfractionResult> BanAsync(ulong userId, ulong guildId, ulong enforcerId, string reason, DateTime? expiration = null)
		{
			var guild = _client.GetShard(guildId).Guilds[guildId];
			var enforcer = guild.Members[enforcerId];
			var userExists = guild.Members.TryGetValue(userId, out var member);
			var embed = CreateUserInfractionEmbed(enforcer, guild.Name, expiration is null ? InfractionType.Ban : InfractionType.SoftBan, reason, expiration);

			var notified = false;

			try
			{
				if (userExists)
				{
					await member?.SendMessageAsync(embed)!;
					notified = true;
				}
			}
			catch (UnauthorizedException) { }

			try
			{
				await guild.BanMemberAsync(userId, 0, reason);

				var inf = await GenerateInfractionAsync(userId, guildId, enforcerId, expiration is null ? InfractionType.Ban : InfractionType.SoftBan, reason, expiration);
				
				if (inf.Duration is not null)
					_infractions.Add(inf);
				
				await LogToModChannel(inf);
				return notified ? InfractionResult.SucceededWithNotification : InfractionResult.SucceededWithoutNotification;
			}
			catch (UnauthorizedException) /*Shouldn't happen, but you know.*/
			{
				return InfractionResult.FailedSelfPermissions;
			}
		}
	    public async Task StrikeAsync(ulong userId, ulong guildId, ulong enforcerId, string reason, bool isAutoMod = false) { }
	    public async ValueTask<bool> IsMutedAsync(ulong userId, ulong guildId)
	    {
		    var memInf = _mutes.Contains((userId, guildId));
		    
		    if (memInf)
			    return true;

		    var dbInf = await _mediator.Send(new GetUserInfractionsRequest(guildId, userId));

		    return dbInf.Any(inf => !inf.Rescinded && inf.Type is InfractionType.Mute or InfractionType.AutoModMute);
	    }
	    public async Task<InfractionResult> MuteAsync(ulong userId, ulong guildId, ulong enforcerId, string reason, DateTime? expiration)
	    {
		    if (await IsMutedAsync(userId, guildId))
		    {
			    /*We're updating a mute*/
			    return InfractionResult.SucceededDoesNotNotify;
		    }
		    
		    DiscordGuild guild = _client.GetShard(guildId).Guilds[guildId];
		    bool exists = guild.Members.TryGetValue(userId, out DiscordMember? member);
		    
		    GuildConfig conf = await _config.GetConfigAsync(guildId);
		    DiscordRole? muteRole = guild.GetRole(conf.MuteRoleId);
		    
		    if (conf.MuteRoleId is 0 || muteRole is null)
			    muteRole = await GenerateMuteRoleAsync(guild);

		    try
		    {
			    await member!.GrantRoleAsync(muteRole, reason);
		    }
		    catch (NotFoundException)
		    {
			    return InfractionResult.FailedMemberGuildCache;
		    }
		    
		    InfractionType infractionType = enforcerId == _client.CurrentUser.Id ? InfractionType.AutoModMute : InfractionType.Mute;
		    InfractionDTO infraction = await GenerateInfractionAsync(userId, guildId, enforcerId, infractionType, reason, expiration);

		    await LogToModChannel(infraction);
		    
		    var notified = false;
		    
		    // ReSharper disable once InvertIf
		    if (exists)
		    {
			    try
			    {
				    DiscordEmbed muteEmbed = CreateUserInfractionEmbed(guild.Members[enforcerId], guild.Name, infractionType, reason, expiration);
				    await member.SendMessageAsync(muteEmbed);
				    notified = true;
			    }
			    catch { /* This could only be unauth'd exception. */ }
		    }

		    _mutes.Add((userId, guildId));
		    _infractions.Add(infraction);
		    
		    return notified ?
			    InfractionResult.SucceededWithNotification : 
			    InfractionResult.SucceededWithoutNotification;
	    }

	    public async Task<InfractionStep> GetCurrentInfractionStepAsync(ulong guildId, IEnumerable<InfractionDTO> infractions)
	    {
		    return null;
	    }
	    public Task<InfractionDTO> GenerateInfractionAsync(ulong userId, ulong guildId, ulong enforcerId, InfractionType type, string reason, DateTime? expiration) 
		    => _mediator.Send(new CreateInfractionRequest(userId, enforcerId, guildId, reason, type, expiration));
	    
	    public async Task<InfractionResult> AddNoteAsync(ulong userId, ulong guildId, ulong noterId, string note)
	    {
		    return InfractionResult.FailedGuildHeirarchy;
	    }
	    public async Task<InfractionResult> UpdateNoteAsync(ulong userId, ulong guildId, ulong noterId, string newNote)
	    {
		    
		    return InfractionResult.FailedGuildHeirarchy;
	    }


	    /// <summary>
	    /// Creates a formatted embed to be sent to a user.
	    /// </summary>
	    /// <param name="enforcer">The user that created this infraction.</param>
	    /// <param name="guildName">The name of the guild the infraction occured on.</param>
	    /// <param name="type">The type of infraction.</param>
	    /// <param name="reason">Why the infraction was created.</param>
	    /// <param name="expiration">When the infraction expires.</param>
	    /// <returns>A <see cref="DiscordEmbed"/> populated with relevant inforamtion.</returns>
	    /// <exception cref="ArgumentException">An unknown infraction type was passed.</exception>
	    private static DiscordEmbedBuilder CreateUserInfractionEmbed(DiscordUser enforcer, string guildName, InfractionType type, string reason, DateTime? expiration = default)
	    {
		    var action = type switch
		    {
			    InfractionType.Kick			=> $"You've been kicked from {guildName}!",
			    InfractionType.Ban			=> $"You've been permenantly banned from {guildName}!",
			    InfractionType.SoftBan		=> $"You've been temporarily banned from {guildName}!",
			    InfractionType.Mute			=> $"You've been muted on {guildName}!",
			    InfractionType.AutoModMute	=> $"You've been automatically muted on {guildName}!",
			    InfractionType.Strike		=> $"You've been warned on {guildName}!",
			    InfractionType.Unmute		=> $"You've been unmuted on {guildName}!",
			    _ => throw new ArgumentException($"Unexpected enum value: {type}")
		    };
		    
		    var embed = new DiscordEmbedBuilder()
				    .WithTitle(action)
					.WithAuthor($"{enforcer.Username}#{enforcer.Discriminator}", enforcer.GetUrl(), enforcer.AvatarUrl)
				    .AddField("Reason:", reason)
				    .AddField("Infraction occured:", 
					    $"{Formatter.Timestamp(TimeSpan.Zero, TimestampFormat.LongDateTime)}\n\n({Formatter.Timestamp(TimeSpan.Zero)})")
				    .AddField("Enforcer:", enforcer.Id.ToString());

		    if (expiration.HasValue)
			    embed.AddField("Expires:", Formatter.Timestamp(expiration.Value));

		    return embed;
	    }
		
	    
	    /// <summary>
	    /// Sends a message to the appropriate log channel that an infraction (note, reason, or duration) was updated.
	    /// </summary>
	    /// <param name="inf"></param>
	    private async Task LogUpdatedInfractionAsync(InfractionDTO inf)
	    {
		    
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
			    .AddField("Created:", Formatter.Timestamp(inf.CreatedAt, TimestampFormat.LongDateTime), true)
			    .AddField("​", "​", true)
			    .AddField("Offender:", $"**{user.ToDiscordName()}**\n(`{user.Id}`)", true)
			    .AddField("Enforcer:", user == _client.CurrentUser ? "[AUTOMOD]" : $"**{enforcer.ToDiscordName()}**\n(`{enforcer.Id}`)", true)
			    .AddField("Reason:", inf.Reason);

		    if (inf.Duration is TimeSpan ts) /* {} (object) pattern is cursed but works. */
			    builder.AddField("Expires:", Formatter.Timestamp(ts));
		    
		    try { await guild.Channels[config.LoggingChannel].SendMessageAsync(builder); }
		    catch (UnauthorizedException) 
		    { 
			    /*
				Log something here, and to the backup channel. 
				-- Update -- I have no idea wth "the backup channel" is. 
				*/ 
		    }
		}
		
		private async Task<DiscordRole> GenerateMuteRoleAsync(DiscordGuild guild)
		{
			var mute = await guild.CreateRoleAsync("Muted", Permissions.None | Permissions.AccessChannels | Permissions.ReadMessageHistory, new("#363636"), false, false, "Mute role was not present on guild");
			await mute.ModifyPositionAsync(guild.CurrentMember.Hierarchy - 1);

			await _mediator.Send(new UpdateGuildConfigRequest(guild.Id) {MuteRoleId = mute.Id});
			return mute;
		}
	    
	    /// <summary>
	    /// Ensures a moderation channel exists. If it doesn't one will be created, and hidden.
	    /// </summary>
	    private async Task EnsureModLogChannelExistsAsync(ulong guildId)
	    {
		    GuildConfig config = await _config.GetConfigAsync(guildId);
		    DiscordGuild guild = _client.GetShard(guildId).Guilds[guildId];
		    
		    if (config.LoggingChannel is 0)
		    {
			    if (!guild.CurrentMember.HasPermission(Permissions.ManageChannels))
				    return; /* We can't create channels. Sad. */

			    try
			    {
				    var overwrites = new DiscordOverwriteBuilder[]
				    {
					    new(guild.EveryoneRole) {Denied = Permissions.AccessChannels},
					    new(guild.CurrentMember) {Allowed = Permissions.AccessChannels}
				    };

				    var chn = await guild.CreateChannelAsync("mod-log", ChannelType.Text, (DiscordChannel) guild.Channels.Values.EntityOfType(ChannelType.Category).Last(), overwrites: overwrites);
				    await chn.SendMessageAsync("A logging channel was not available when this infraction was created, so one has been generated.");
				    await _mediator.Send(new UpdateGuildConfigRequest(guildId) {LoggingChannel = chn.Id});
				    _updater.UpdateGuild(guildId);
			    }
			    catch { /* Igonre. We can't do anything about it :( */ }
		    }
	    }
    }
}