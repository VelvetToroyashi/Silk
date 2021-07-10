using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using Humanizer;
using MediatR;
using Microsoft.Extensions.Hosting;
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
    public sealed class InfractionService : IHostedService, IInfractionService
    {
	    private readonly ILogger<IInfractionService> _logger;
	    private readonly IMediator _mediator;
	    private readonly DiscordShardedClient _client;

	    private readonly ConfigService _config;
	    private readonly ICacheUpdaterService _updater;

	    private readonly ConcurrentDictionary<ulong, SemaphoreSlim> _semaphoreDict = new();
	    private readonly AsyncTimer _timer;
	    
	    private readonly InfractionStep _ignoreStep = new() {Type = InfractionType.Ignore};
	    
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
		    _timer = new(DequeueInfractionsAsync, TimeSpan.FromSeconds(1), true);
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
		    catch (NotFoundException) { return InfractionResult.FailedGuildMemberCache; }
		    catch (UnauthorizedException) { return InfractionResult.FailedSelfPermissions; } /* This shouldn't apply, but. */

		    var inf = await GenerateInfractionAsync(userId, guildId, enforcerId, InfractionType.Kick, reason, null);

		    await LogInfractionAsync(inf);
		    return notified ? InfractionResult.SucceededWithNotification : InfractionResult.SucceededWithoutNotification; 
		}

		public async Task<InfractionResult> BanAsync(ulong userId, ulong guildId, ulong enforcerId, string reason, DateTime? expiration = null)
		{
			var guild = _client.GetShard(guildId).Guilds[guildId];
			var enforcer = guild.Members[enforcerId];
			var userExists = guild.Members.TryGetValue(userId, out var member);
			var embed = CreateUserInfractionEmbed(enforcer, guild.Name, expiration is null ? InfractionType.Ban : InfractionType.SoftBan, reason, expiration);

			if ((member?.Hierarchy ?? -1) > enforcer.Hierarchy)
				return InfractionResult.FailedGuildHeirarchy;

			if ((member?.Hierarchy ?? -1) > guild.CurrentMember.Hierarchy)
				return InfractionResult.FailedGuildHeirarchy;
				
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
				
				await LogInfractionAsync(inf);
				return notified ? 
					InfractionResult.SucceededWithNotification :
					InfractionResult.SucceededWithoutNotification;
			}
			catch (UnauthorizedException) /*Shouldn't happen, but you know.*/
			{
				return InfractionResult.FailedSelfPermissions;
			}
		}
		
		public async Task<InfractionResult> UnBanAsync(ulong userId, ulong guildId, ulong enforcerId, string reason = "Not Given.")
		{
			var guild = _client.GetShard(guildId).Guilds[guildId];
			
			if (!guild.CurrentMember.HasPermission(Permissions.BanMembers))
				return InfractionResult.FailedSelfPermissions;
			try
			{
				await guild.UnbanMemberAsync(userId, reason);
			}
			catch (NotFoundException)
			{
				return InfractionResult.FailedGuildMemberCache; // User isn't banned. //
			}

			_infractions.RemoveAll(inf => inf.Type is InfractionType.Ban or InfractionType.SoftBan && inf.UserId == userId);
			
			var userInfractions = await _mediator.Send(new GetUserInfractionsRequest(guildId, userId));
			var banInfraction = userInfractions.OrderByDescending(inf => inf.CreatedAt).FirstOrDefault(inf => inf.Type is InfractionType.Ban or InfractionType.SoftBan);
			await _mediator.Send(new UpdateInfractionRequest(banInfraction.Id, banInfraction.Expiration, banInfraction.Reason, true));
			
			//TODO: Make expiration parameter optional because it bugs me ~Velvet
			var infraction = await GenerateInfractionAsync(userId, guildId, enforcerId, InfractionType.Unban, reason, null);
			await LogInfractionAsync(infraction);
			
			return InfractionResult.SucceededDoesNotNotify;
		}

		public async Task<InfractionResult> StrikeAsync(ulong userId, ulong guildId, ulong enforcerId, string reason, bool autoEscalate = false)
		{
			var guild = _client.GetShard(guildId).Guilds[guildId];
			var enforcer = guild.Members[enforcerId];
			var exists = guild.Members.TryGetValue(userId, out var victim);
			
			
			if (!autoEscalate)
			{
				var infraction = await GenerateInfractionAsync(userId, guildId, enforcerId, InfractionType.Strike, reason, null);
				var embed = CreateUserInfractionEmbed(enforcer, guild.Name, InfractionType.Strike, reason);

				var logResult = await LogInfractionAsync(infraction);

				if (logResult is InfractionResult.FailedLogPermissions)
					return logResult;
				
				if (!exists) 
					return InfractionResult.FailedGuildMemberCache;
				
				try
				{
					await victim!.SendMessageAsync(embed);
					return InfractionResult.SucceededWithNotification;
				}
				catch (UnauthorizedException)
				{
					return InfractionResult.SucceededWithoutNotification;
				}
			}
			
			var infractions = await _mediator.Send(new GetUserInfractionsRequest(guildId, userId));
			var step = await GetCurrentInfractionStepAsync(guildId, infractions);

			var task = step.Type switch
			{
				InfractionType.Ignore when enforcer == _client.CurrentUser => AddNoteAsync(userId, guildId, enforcerId, reason),
				InfractionType.Ignore when enforcer != _client.CurrentUser => BanAsync(userId, guildId, enforcerId, reason),
				InfractionType.Kick => KickAsync(userId, guildId, enforcerId, reason),
				InfractionType.Ban => BanAsync(userId, guildId, enforcerId, reason),
				InfractionType.SoftBan => BanAsync(userId, guildId, enforcerId, reason, DateTime.UtcNow + step.Duration.Time),
				InfractionType.Mute => MuteAsync(userId, guildId, enforcerId, reason, DateTime.Now + step.Duration.Time),
				_ => Task.FromResult(InfractionResult.SucceededDoesNotNotify)
			};
			
			var res = await task;
			var lastInfraction = _infractions.Last(inf => inf.UserId == userId && inf.Reason == reason);
			await _mediator.Send(new UpdateInfractionRequest(lastInfraction.Id, lastInfraction.Expiration, reason, false, true));

			return res;
		}

		public async ValueTask<bool> IsMutedAsync(ulong userId, ulong guildId)
		{
			EnsureSemaphoreExists(guildId);
			await _semaphoreDict[guildId].WaitAsync();
			
			try
			{
				var isInMemory = _mutes.Contains((userId, guildId));

				if (isInMemory)
					return true;

				var dbInf = await _mediator.Send(new GetUserInfractionsRequest(guildId, userId));
				var inf = dbInf.SingleOrDefault(inf => inf.HeldAgainstUser && inf.Type is InfractionType.Mute or InfractionType.AutoModMute);

				// ReSharper disable once InvertIf
				if (inf is not null)
				{
					_infractions.Add(inf);
					_mutes.Add((userId, guildId));
				}

				return inf is not null;
			}
			finally
			{
				_semaphoreDict[guildId].Release();
			}
		}

		public async Task<InfractionResult> MuteAsync(ulong userId, ulong guildId, ulong enforcerId, string reason, DateTime? expiration)
	    {
		    DiscordGuild guild = _client.GetShard(guildId).Guilds[guildId];
		    GuildConfig conf = await _config.GetConfigAsync(guildId);
		    DiscordRole? muteRole = guild.GetRole(conf!.MuteRoleId);
		    
			if (await IsMutedAsync(userId, guildId))
			{
				InfractionDTO? memInfraction = _infractions.Find(inf => inf.UserId == userId && inf.Type is InfractionType.Mute or InfractionType.AutoModMute);
				/* Repplying mute */
								
				if (expiration != memInfraction!.Expiration)
				{
					var newInf = await _mediator.Send(new UpdateInfractionRequest(memInfraction!.Id, expiration, reason));
					await LogUpdatedInfractionAsync(memInfraction, newInf);

					_ = memInfraction = newInf; 
					/*
						All that's really necessary is inf = newInf;
						I don't know why Roslyn analyzers don't pick up on this "usage"; it says it's assigned but never used
						But if you were to do: inf = newInf; _ = inf;
						Analyzers are more than happy to say "Yep! This variable is being used. 
						Anyway thanks for coming to my TedTalk
					 */
				}
				
				muteRole ??= await GenerateMuteRoleAsync(guild, guild.Members[userId]);
				/* It *should* be almost impossible for someone to leave a server this fast without selfbotting */
				try
				{
					var mem = guild.Members[userId];
					
					if (!mem.Roles.Contains(muteRole))
						await mem.GrantRoleAsync(muteRole, "Reapplying active mute.");
				}
				catch (NotFoundException) { }
				catch (ServerErrorException) { }
				catch (UnauthorizedException) { }
				
				
				return InfractionResult.SucceededDoesNotNotify;
			}
		    
		    
		    bool exists = guild.Members.TryGetValue(userId, out DiscordMember? member);
		    
		    if (conf.MuteRoleId is 0 || muteRole is null)
			    muteRole = await GenerateMuteRoleAsync(guild, member!);
		    
		    InfractionType infractionType = enforcerId == _client.CurrentUser.Id ? InfractionType.AutoModMute : InfractionType.Mute;
		    InfractionDTO infraction = await GenerateInfractionAsync(userId, guildId, enforcerId, infractionType, reason, expiration);

		    await LogInfractionAsync(infraction);
		    _mutes.Add((userId, guildId));
		    _infractions.Add(infraction);
		    
		    try
		    {
			    await member!.GrantRoleAsync(muteRole, reason);
		    }
		    catch (NotFoundException)
		    {
			    return InfractionResult.FailedGuildMemberCache;
		    }
		    
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
		    
		    return notified ?
			    InfractionResult.SucceededWithNotification : 
			    InfractionResult.SucceededWithoutNotification;
	    }
		public async Task<InfractionResult> UnMuteAsync(ulong userId, ulong guildId, ulong enforcerId, string reason = "Not Given.")
		{
			if (!await IsMutedAsync(userId, guildId))
			    return InfractionResult.FailedGenericRequirementsNotFulfilled;
			
		    await _semaphoreDict[guildId].WaitAsync();
		    
		    var index = _infractions.FindIndex(inf => inf.UserId == userId && inf.GuildId == guildId && inf.Type is InfractionType.Mute or InfractionType.AutoModMute);
		    var infraction = _infractions[index];
		    
		    _infractions.RemoveAt(index);
		    _mutes.Remove((userId, guildId));

		    await _mediator.Send(new UpdateInfractionRequest(infraction.Id, infraction.Expiration, infraction.Reason, true)); // Only set it to say it's handled. //
		    var unmute = await GenerateInfractionAsync(userId, guildId, enforcerId, InfractionType.Unmute, reason, null);

		    var guild = _client.GetShard(guildId).Guilds[guildId];
		    if (guild.Members.TryGetValue(userId, out var member))
		    {
			    var conf = await _config.GetConfigAsync(guildId);
			    var role = guild.Roles[conf.MuteRoleId];
			    await member.RevokeRoleAsync(role);

			    var embed = CreateUserInfractionEmbed(guild.Members[enforcerId], guild.Name, InfractionType.Unmute, reason);

			    try
			    {
				    await member.SendMessageAsync(embed);
				    return InfractionResult.SucceededWithNotification;
			    }
			    catch (UnauthorizedException) { return InfractionResult.SucceededWithoutNotification; }
			    finally
			    {
				    await _mediator.Send(new UpdateInfractionRequest(infraction.Id, DateTime.UtcNow, infraction.Reason, true));
				    await LogInfractionAsync(unmute);
				    _semaphoreDict[guildId].Release();
			    }
		    }

		    await _mediator.Send(new UpdateInfractionRequest(infraction.Id, DateTime.UtcNow, infraction.Reason, true));
		    await LogInfractionAsync(unmute);
		    
		    _semaphoreDict[guildId].Release();
		    return InfractionResult.SucceededDoesNotNotify;
		}

	    public async Task<InfractionStep> GetCurrentInfractionStepAsync(ulong guildId, IEnumerable<InfractionDTO> infractions)
		{
		    GuildConfig conf = await _config.GetConfigAsync(guildId);
		    if (!conf.InfractionSteps.Any())
			    return _ignoreStep;
		    
		    var infractionCount = GetElegibleInfractions(infractions);
		    var infLevels = conf.InfractionSteps;

		    int index = Math.Max(infLevels.Count - 1, infractionCount - 1);

		    return infLevels[index];
		    int GetElegibleInfractions(IEnumerable<InfractionDTO> inf) => inf.Count(i => !i.HeldAgainstUser && i.EnforcerId == _client.CurrentUser.Id && i.Type is InfractionType.Strike);
		}
	    
	    public Task<InfractionDTO> GenerateInfractionAsync(ulong userId, ulong guildId, ulong enforcerId, InfractionType type, string reason, DateTime? expiration) 
		    => _mediator.Send(new CreateInfractionRequest(userId, enforcerId, guildId, reason, type, expiration));
	    
	    public async Task<InfractionResult> AddNoteAsync(ulong userId, ulong guildId, ulong noterId, string note)
	    {
		    var infractionNote = await GenerateInfractionAsync(userId, guildId, noterId, InfractionType.Note, note, null);
		    
		    var mainNoteEmbed = new DiscordEmbedBuilder();
		    var guild = _client.GetShard(guildId).Guilds[guildId];

		    DiscordUser? user;
		    _ = guild.Members.TryGetValue(noterId, out var enforcer);
		    _ = guild.Members.TryGetValue(userId, out var tmem);
		    user = tmem;
		    user ??= await _client.ShardClients[0].GetUserAsync(userId);

		    mainNoteEmbed
			    .WithAuthor($"{user.Username}#{user.Discriminator}", user.GetUrl(), user.AvatarUrl)
			    .WithThumbnail(enforcer!.AvatarUrl, 4096, 4096)
			    .WithDescription("A new case has been added to this guild's list of infractions.")
			    .WithColor(DiscordColor.Gold)
			    .AddField("Type:", infractionNote.Type.Humanize(LetterCasing.Title), true)
			    .AddField("Created:", Formatter.Timestamp(infractionNote.CreatedAt, TimestampFormat.LongDateTime), true)
			    .AddField("Case Id:", $"#{infractionNote.CaseNumber}", true)
			    .AddField("User:", $"**{user.ToDiscordName()}**\n(`{user.Id}`)", true)
			    .AddField("Noted by:", user == _client.CurrentUser ? "[AUTOMOD]" : $"**{enforcer.ToDiscordName()}**\n(`{enforcer.Id}`)", true);

		    var noteReasonEmbed = new DiscordEmbedBuilder()
			    .WithDescription(note)
			    .WithColor(DiscordColor.Gold)
			    .WithTitle($"Note (case {infractionNote.CaseNumber})");
		    
		    await EnsureModLogChannelExistsAsync(guildId);
		    var conf = await _config.GetConfigAsync(guildId);
		    if (conf.LoggingChannel is 0)
			    return InfractionResult.FailedNotConfigured;
		    var channel = guild.Channels[conf.LoggingChannel];
		    
			if (!channel.PermissionsFor(guild.CurrentMember).HasPermission(Permissions.SendMessages | Permissions.EmbedLinks))
			    return InfractionResult.FailedLogPermissions;

			try
			{
				await channel.SendMessageAsync(buil => buil.AddEmbeds(new DiscordEmbed[] {mainNoteEmbed, noteReasonEmbed}));
			}
			catch { /* ??? */ }
		    
		    return InfractionResult.SucceededDoesNotNotify;
	    }

	    public async Task<InfractionResult> PardonAsync(ulong userId, ulong guildId, ulong enforcerId, string reason = "Not Given.")
	    {
		    var infractions = await _mediator.Send(new GetUserInfractionsRequest(guildId, userId));
		    var rescindedInfraction = infractions.OrderBy(inf => inf.CreatedAt).LastOrDefault(inf => inf.HeldAgainstUser && inf.Type is InfractionType.Strike || inf.EscalatedFromStrike);
		    
		    if (rescindedInfraction is null)
			    return InfractionResult.FailedGenericRequirementsNotFulfilled;
		    
		    await _mediator.Send(new UpdateInfractionRequest(rescindedInfraction.Id, rescindedInfraction.Expiration, rescindedInfraction.Reason, true));

		    var infraction = await GenerateInfractionAsync(userId, guildId, enforcerId, InfractionType.Pardon, reason, null);
		    
		    await EnsureModLogChannelExistsAsync(guildId);
		    await LogInfractionAsync(infraction);

		    var guild = _client.GetShard(guildId).Guilds[guildId];
		    
		    if (!guild.Members.TryGetValue(userId, out var member)) 
			    return InfractionResult.SucceededDoesNotNotify;
		    
		    var user = await _client.ShardClients[0].GetUserAsync(enforcerId);
		    try
		    {
			    await member.SendMessageAsync(CreateUserInfractionEmbed(user, guild.Name, InfractionType.Pardon, reason));
			    return InfractionResult.SucceededWithNotification;
		    }
		    catch
		    {
			    return InfractionResult.SucceededWithoutNotification;
		    }
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
			    InfractionType.Pardon		=> $"You've been pardoned from one (1) infraction on {guildName}!",
			    InfractionType.Ignore or InfractionType.Note => null,
			    _ => throw new ArgumentException($"Unexpected enum value: {type}")
		    };
		    
		    var embed = new DiscordEmbedBuilder()
				    .WithTitle(action)
					.WithAuthor($"{enforcer.Username}#{enforcer.Discriminator}", enforcer.GetUrl(), enforcer.AvatarUrl)
				    .AddField("Reason:", reason)
				    .AddField("Infraction occured:", 
					    $"{Formatter.Timestamp(TimeSpan.Zero, TimestampFormat.LongDateTime)}\n\n({Formatter.Timestamp(TimeSpan.Zero)})")
				    .AddField("Enforcer:", enforcer.Id.ToString());

		    
		    if (!expiration.HasValue && type is InfractionType.Mute or InfractionType.AutoModMute)
			    embed.AddField("Expires:", "This infraction does not have an expiry date.");
		    
		    else if (expiration.HasValue)
			    embed.AddField("Expires:", Formatter.Timestamp(expiration.Value));

		    
		    
		    return embed;
	    }


	    /// <summary>
	    /// Sends a message to the appropriate log channel that an infraction (note, reason, or duration) was updated.
	    /// </summary>
	    private async Task<InfractionResult> LogUpdatedInfractionAsync(InfractionDTO infOld, InfractionDTO infNew)
	    {
		    await EnsureModLogChannelExistsAsync(infNew.GuildId);
		    var conf = await _config.GetConfigAsync(infNew.GuildId);

		    var modLog = conf.LoggingChannel;
		    var guild = _client.GetShard(infNew.GuildId).Guilds[infNew.GuildId];

		    if (modLog is 0)
			    return InfractionResult.FailedNotConfigured;
		    
		    if (!guild.Channels.TryGetValue(modLog, out var chn))
			    return InfractionResult.FailedResourceDeleted;

		    var user = await _client.ShardClients[0].GetUserAsync(infOld.UserId); /* User may not exist on the server anymore. */
		    var enforcer = _client.GetShard(infNew.GuildId).Guilds[infNew.GuildId].Members[infNew.EnforcerId];

		    var infractionEmbeds = GenerateUpdateEmbed(user, enforcer, infOld, infNew);

		    try
		    {
			    var builder = new DiscordMessageBuilder();
			    
			    builder.AddEmbeds(infractionEmbeds);
			    
			     await chn.SendMessageAsync(builder);
		    }
		    catch (UnauthorizedException)
		    {
			    return InfractionResult.FailedLogPermissions;
		    }
		    return InfractionResult.SucceededDoesNotNotify;


		    IEnumerable<DiscordEmbed> GenerateUpdateEmbed(DiscordUser victim, DiscordUser enforcer, InfractionDTO infractionOLD, InfractionDTO infractionNEW)
		    {
			    var builder = new DiscordEmbedBuilder();
			    builder
				    .WithColor(DiscordColor.Gold)
				    .WithThumbnail(enforcer.AvatarUrl, 4096, 4096)
				    .WithTitle("An infraction in this guild has been updated.")
				    .WithAuthor($"{victim.Username}#{victim.Discriminator}", victim.GetUrl(), victim.AvatarUrl)
				    .AddField("Type:", infractionOLD.Type.Humanize(LetterCasing.Title), true)
				    .AddField("Created:", Formatter.Timestamp(infractionOLD.CreatedAt, TimestampFormat.LongDateTime), true)
				    .AddField("Case Id:", $"#{infractionOLD.CaseNumber}", true)
				    .AddField("Offender:", $"**{victim.ToDiscordName()}**\n(`{victim.Id}`)", true)
				    .AddField("Enforcer:", enforcer == _client.CurrentUser ? "[AUTOMOD]" : $"**{enforcer.ToDiscordName()}**\n(`{enforcer.Id}`)", true);
					
			    
			    if (infractionOLD.Duration.HasValue || infractionNEW.Duration.HasValue)
			    {
				    string expiry = (infractionOLD.Duration, infractionNEW.Duration) switch
				    {
					    (TimeSpan t1, TimeSpan t2) => $"{Formatter.Timestamp(t1, TimestampFormat.LongDateTime)} → {Formatter.Timestamp(t2, TimestampFormat.LongDateTime)}",
					    (TimeSpan t, null) => $"{Formatter.Timestamp(t, TimestampFormat.LongDateTime)} → Never",
					    (null, TimeSpan t) => $"Never → {Formatter.Timestamp(t, TimestampFormat.LongDateTime)}",
					    (null, null) => "Never"
				    };
				    builder.AddField("Expiration", expiry);
			    }
			    
			    if (infractionOLD.Reason.Length < 2000 && infractionNEW.Reason.Length < 2000)
			    {
				    if (!string.Equals(infractionOLD.Reason, infractionNEW.Reason))
					    builder.WithDescription($"Old reason: ```\n\n{infractionOLD.Reason}``` \n\nNew reason: ```\n{infractionNEW.Reason}```");
				    return new DiscordEmbed[] {builder};
			    }
			    
			    builder.WithFooter("The reason was over 2000 characters, and have been added to a second embed!");
			    var b2 = new DiscordEmbedBuilder().WithColor(DiscordColor.Gold).WithDescription(infractionNEW.Reason);
			    return new DiscordEmbed[] {builder, b2};
		    }
	    }
	    
		/// <summary>
		/// Logs to the designated mod-log channel, if any.
		/// </summary>
		/// <param name="inf">The infraction to log.</param>
		private async Task<InfractionResult> LogInfractionAsync(InfractionDTO inf)
		{ 
			await EnsureModLogChannelExistsAsync(inf.GuildId);
		    
		    var config = await _config.GetConfigAsync(inf.GuildId);
		    var guild = _client.GetShard(inf.GuildId).Guilds[inf.GuildId];
		    
		    if (config.LoggingChannel is 0)
			    return InfractionResult.FailedNotConfigured; /* It couldn't create a mute channel :(*/

		    var user = await _client.ShardClients[0].GetUserAsync(inf.UserId); /* User may not exist on the server anymore. */
		    var enforcer = _client.GetShard(inf.GuildId).Guilds[inf.GuildId].Members[inf.EnforcerId];
		    
		    var builder = new DiscordEmbedBuilder();

		    builder
			    .WithAuthor(user.ToDiscordName(), user.GetUrl(), user.AvatarUrl)
			    .WithThumbnail(enforcer.AvatarUrl, 4096, 4096)
			    .WithDescription("A new case has been added to this guild's list of infractions.")
			    .WithColor(DiscordColor.Gold)
			    .AddField("Type:", inf.Type.Humanize(LetterCasing.Title), true)
			    .AddField("Created:", Formatter.Timestamp(inf.CreatedAt, TimestampFormat.LongDateTime), true)
			    .AddField("Case Id:", $"#{inf.CaseNumber}", true)
			    .AddField("Offender:", $"**{user.ToDiscordName()}**\n(`{user.Id}`)", true)
			    .AddField("Enforcer:", user == _client.CurrentUser ? "[AUTOMOD]" : $"**{enforcer.ToDiscordName()}**\n(`{enforcer.Id}`)", true)
			    .AddField("Reason:", inf.Reason);

		    if (inf.Duration is TimeSpan ts) /* {} (object) pattern is cursed but works. */
			    builder.AddField("Expires:", Formatter.Timestamp(ts));
		    
		    try { await guild.Channels[config.LoggingChannel].SendMessageAsync(builder); }
		    catch (UnauthorizedException)
		    {
			    return InfractionResult.FailedLogPermissions;
		    }
		    return InfractionResult.SucceededDoesNotNotify;
		}
		
		private async Task<DiscordRole> GenerateMuteRoleAsync(DiscordGuild guild, DiscordMember member)
		{
			if (!guild.CurrentMember.Permissions.HasPermission(Permissions.ManageRoles))
				throw new InvalidOperationException("Current member does not have permission to create roles.");
			
			if (!guild.CurrentMember.Permissions.HasPermission(Permissions.ManageChannels))
				throw new InvalidOperationException("Current member does not have permission to manage channels.");
			
			
			var channels = guild.Channels.Values
				.OfType(ChannelType.Text)
				.Where(c => guild.CurrentMember.PermissionsIn(c).HasPermission(Permissions.ManageChannels | Permissions.AccessChannels | Permissions.SendMessages | Permissions.ManageRoles))
				.ToArray();
			
			foreach (var role in guild.Roles.Values)
			{
				if (role.Position <= member.Hierarchy)
					continue;

				if (member.Roles.Contains(role))
					continue;
				
				if (!channels.All(r => r.PermissionOverwrites.Any(p => p.Id == role.Id))) 
					continue;
			
				await _mediator.Send(new UpdateGuildConfigRequest(guild.Id) {MuteRoleId = role.Id});
				return role;
			}
			
			DiscordRole mute = await guild.CreateRoleAsync("Muted", null, new("#363636"), false, false, "Mute role was not present on guild");
			await mute.ModifyPositionAsync(guild.CurrentMember.Hierarchy - 1);

			foreach (var c in channels)
			{
				if (!c.PermissionsFor(member).HasPermission(Permissions.SendMessages | Permissions.AccessChannels))
					continue;
				await c.AddOverwriteAsync(mute, Permissions.None, Permissions.SendMessages);
			}
			
			await _mediator.Send(new UpdateGuildConfigRequest(guild.Id) {MuteRoleId = mute.Id});
			_updater.UpdateGuild(guild.Id);
			return mute;
		}
	    
		/// <summary>
		/// Ensures a moderation channel exists. If it doesn't one will be created, and hidden.
		/// </summary>
		private async Task EnsureModLogChannelExistsAsync(ulong guildId)
		{
		    GuildConfig config = await _config.GetConfigAsync(guildId);
		    DiscordGuild guild = _client.GetShard(guildId).Guilds[guildId];

		    if (config.LoggingChannel is not 0)
			    return;
		    
		    if (!guild.CurrentMember.HasPermission(Permissions.ManageChannels))
			    return; /* We can't create channels. Sad. */

		    try
		    {
			    var overwrites = new DiscordOverwriteBuilder[]
			    {
				    new(guild.EveryoneRole) {Denied = Permissions.AccessChannels},
				    new(guild.CurrentMember) {Allowed = Permissions.AccessChannels}
			    };

			    var chn = await guild.CreateChannelAsync("mod-log", ChannelType.Text, guild.Channels.Values.OfType(ChannelType.Category).Last(), overwrites: overwrites);
			    await chn.SendMessageAsync("A logging channel was not available when this infraction was created, so one has been generated.");
			    await _mediator.Send(new UpdateGuildConfigRequest(guildId) {LoggingChannel = chn.Id});
			    _updater.UpdateGuild(guildId);
		    }
		    catch { /* Igonre. We can't do anything about it :( */ }
		}
		public async Task StartAsync(CancellationToken cancellationToken)
		{
			_logger.LogInformation("Started!");
			var now = DateTime.UtcNow;
			await Task.Yield();

			_logger.LogInformation("Loading infractions");
			var infractionsTask = _mediator.Send(new GetCurrentInfractionsRequest(), cancellationToken);
			var delayTask = Task.Delay(200, cancellationToken);

			if (await Task.WhenAny(infractionsTask, delayTask) != delayTask)
			{
				await LoadAndCacheInfractionsAsync();
			}
			else
			{
				_logger.LogWarning("Slow load for infractions. Offloading to ThreadPool.");
				_ = Task.Run(async () => await LoadAndCacheInfractionsAsync(), cancellationToken);
			}

			async Task LoadAndCacheInfractionsAsync()
			{
				var allInfractions = await infractionsTask;
				foreach (var infraction in allInfractions)
				{
					if (infraction.Type is InfractionType.AutoModMute or InfractionType.Mute)
						_mutes.Add((infraction.UserId, infraction.GuildId));
					_infractions.Add(infraction);
				}
				var tsNow = DateTime.UtcNow - now;
				_logger.LogInformation("Loaded {Infractions} infractions in {Time} ms", allInfractions.Count(), tsNow.TotalMilliseconds.ToString("N0"));
				_timer.Start();
			}
		}
		public async Task StopAsync(CancellationToken cancellationToken)
		{
			_timer.Dispose(); // Stops the timer. It's fine. //
		}


		private void EnsureSemaphoreExists(ulong guildId)
		{
			if (!_semaphoreDict.TryGetValue(guildId, out _))
				_semaphoreDict[guildId] = new(1);
		}
		
		private async Task DequeueInfractionsAsync()
		{
			foreach (var infraction in _infractions) 
				if (infraction.Expiration < DateTime.UtcNow)
					ThreadPool.QueueUserWorkItem(DequeueTask, (this, infraction));

			async void DequeueTask(object? infract)
			{
				var id = _client.CurrentUser.Id;
				(InfractionService service, InfractionDTO infraction) = (ValueTuple<InfractionService, InfractionDTO>)infract!;

				var guildId = infraction.GuildId;
				var task = infraction.Type switch
				{
					InfractionType.Mute or InfractionType.AutoModMute => service.UnMuteAsync(infraction.UserId, guildId, id, "Automatic unmute | This infraction has expired."),
					InfractionType.SoftBan => service.UnBanAsync(infraction.UserId, guildId, id, "Automatic unban | This infraction has expired."),
					_ => Task.CompletedTask
				};
				await task;
			}
		}
    }
}