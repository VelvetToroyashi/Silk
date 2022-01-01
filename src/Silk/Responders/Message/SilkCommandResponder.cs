using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Remora.Commands.Services;
using Remora.Commands.Tokenization;
using Remora.Commands.Trees;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Responders;
using Remora.Discord.Commands.Services;
using Remora.Results;
using Silk.Services.Interfaces;

namespace Silk.Core.Remora.Services;

/*
 * I'm not entirely sure if this is worth breaking out into a "service".
 *
 * Sure, some responders are literally pass-throughs to some of Silk's services,
 * but this is is just a command responder, and pretty innocuous.
 *
 * The benefit of breaking this apart is likely no-existent, and will only hinder
 * readability.
 */
public class SilkCommandResponder : CommandResponder
{
	private readonly IPrefixCacheService _prefixCacheService;
	private readonly IDiscordRestUserAPI _users;

	public SilkCommandResponder
	(
		CommandService                    commandService,
		IOptions<CommandResponderOptions> options,
		ExecutionEventCollectorService    eventCollector,
		IServiceProvider                  services,
		ContextInjectionService           contextInjection,
		IOptions<TokenizerOptions>        tokenizerOptions,
		IOptions<TreeSearchOptions>       treeSearchOptions,
		IPrefixCacheService               prefixCacheService, 
		IDiscordRestUserAPI users
	) : base
	(
		commandService,
		options,
		eventCollector,
		services,
		contextInjection,
		tokenizerOptions,
		treeSearchOptions
	 )
	{
		_prefixCacheService  = prefixCacheService;
		_users          = users;
		options.Value.Prefix = null; // We'll trim prior to searching for prefixes
	}

	public override async Task<Result> RespondAsync(IMessageCreate? gatewayEvent, CancellationToken ct = default)
	{
		if (gatewayEvent is null)
			return Result.FromSuccess();

		string prefix;

		var selfResult = await _users.GetCurrentUserAsync(ct);
		
		if (!selfResult.IsSuccess)
			return Result.FromError(new InvalidOperationError("Could not get self user"));
		
		var self = selfResult.Entity;
		
		if (gatewayEvent.Content.StartsWith("<@") && gatewayEvent.Mentions.FirstOrDefault()?.ID == self.ID)
		{
			// In case a nickname is set, 
			prefix = gatewayEvent.Content.Substring(0, gatewayEvent.Content.IndexOf('>') + 1);
		}
		else
		{
			prefix = _prefixCacheService.RetrievePrefix(gatewayEvent.GuildID.IsDefined(out var guildId) ? guildId : null);

			if (!string.IsNullOrEmpty(prefix) && !gatewayEvent.Content.StartsWith(prefix))
				return Result.FromSuccess(); // Not a command
		}

		if (string.Equals(gatewayEvent.Content.Trim(), prefix, StringComparison.OrdinalIgnoreCase))
			return Result.FromSuccess(); // We're being pinged, but no command is being invoked.
		
		var author = gatewayEvent.Author;

		if (author.IsBot.IsDefined(out var isBot) && isBot)
			return Result.FromSuccess();

		if (author.IsSystem.IsDefined(out var isSystem) && isSystem)
			return Result.FromSuccess();

		var context = new MessageContext //TODO: Replace with .CreateContext() when https://github.com/Nihlus/Remora.Discord/pull/137 gets merged
			(
			 gatewayEvent.ChannelID,
			 author,
			 gatewayEvent.ID,
			 new PartialMessage
				 (
				  gatewayEvent.ID,
				  gatewayEvent.ChannelID,
				  gatewayEvent.GuildID,
				  new(gatewayEvent.Author),
				  gatewayEvent.Member,
				  gatewayEvent.Content[prefix.Length..],
				  gatewayEvent.Timestamp,
				  gatewayEvent.EditedTimestamp,
				  gatewayEvent.IsTTS,
				  gatewayEvent.MentionsEveryone,
				  new(gatewayEvent.Mentions),
				  new(gatewayEvent.MentionedRoles),
				  gatewayEvent.MentionedChannels,
				  new(gatewayEvent.Attachments),
				  new(gatewayEvent.Embeds),
				  gatewayEvent.Reactions,
				  gatewayEvent.Nonce,
				  gatewayEvent.IsPinned,
				  gatewayEvent.WebhookID,
				  gatewayEvent.Type,
				  gatewayEvent.Activity,
				  gatewayEvent.Application,
				  gatewayEvent.ApplicationID,
				  gatewayEvent.MessageReference,
				  gatewayEvent.Flags,
				  gatewayEvent.ReferencedMessage,
				  gatewayEvent.Interaction,
				  gatewayEvent.Thread,
				  gatewayEvent.Components,
				  gatewayEvent.StickerItems
				 ));

		return await base.ExecuteCommandAsync(gatewayEvent.Content[prefix.Length..], context, ct);
	}
}