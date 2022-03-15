using System.Threading;
using System.Threading.Tasks;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.Caching;
using Remora.Discord.Caching.Services;
using Remora.Discord.Gateway.Responders;
using Remora.Results;

namespace Silk.Responders.Message;

[ResponderGroup(ResponderGroup.Late)]
public class LateBoundMessageRecacher : IResponder<IMessageUpdate>
{
    private readonly CacheService _cache;
    public LateBoundMessageRecacher(CacheService cache) => _cache = cache;

    public Task<Result> RespondAsync(IMessageUpdate gatewayEvent, CancellationToken ct = default)
    {
        if (!gatewayEvent.ChannelID.IsDefined(out var channel))
            return Task.FromResult(Result.FromSuccess());
        
        if (!gatewayEvent.ID.IsDefined(out var message))
            return Task.FromResult(Result.FromSuccess());

        var key = KeyHelpers.CreateMessageCacheKey(channel, message);

        if (!_cache.TryGetValue<IMessage>(key, out var cached))
            return Task.FromResult(Result.FromSuccess());

        var updated = new Remora.Discord.API.Objects.Message
            (
             message,
             channel,
             gatewayEvent.GuildID,
             cached.Author,
             cached.Member,
             gatewayEvent.Content.IsDefined(out var content) ? content : cached.Content,
             cached.Timestamp,
             gatewayEvent.EditedTimestamp.HasValue ? gatewayEvent.EditedTimestamp.Value : cached.EditedTimestamp,
             cached.IsTTS,
             cached.MentionsEveryone,
             gatewayEvent.Mentions.IsDefined(out var mentions) ? mentions : cached.Mentions,
             gatewayEvent.MentionedRoles.IsDefined(out var mentionRoles) ? mentionRoles : cached.MentionedRoles,
             gatewayEvent.MentionedChannels.HasValue ? gatewayEvent.MentionedChannels : cached.MentionedChannels,
             gatewayEvent.Attachments.IsDefined(out var attachments) ? attachments : cached.Attachments,
             gatewayEvent.Embeds.IsDefined(out var embeds) ? embeds : cached.Embeds,
             gatewayEvent.Reactions.HasValue ? gatewayEvent.Reactions : cached.Reactions,
             cached.Nonce,
             gatewayEvent.IsPinned.IsDefined(out var isPinned) ? isPinned : cached.IsPinned,
             cached.WebhookID,
             cached.Type,
             cached.Activity,
             cached.Application,
             cached.ApplicationID,
             cached.MessageReference,
             gatewayEvent.Flags.HasValue ? gatewayEvent.Flags : cached.Flags,
             cached.ReferencedMessage,
             cached.Interaction,
             cached.Thread,
             gatewayEvent.Components.HasValue ? gatewayEvent.Components : cached.Components,
             cached.StickerItems
            );
        
        _cache.Cache(key, updated);

        return Task.FromResult(Result.FromSuccess());
    }
}