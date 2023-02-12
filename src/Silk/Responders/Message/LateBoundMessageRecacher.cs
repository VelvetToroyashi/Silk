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

    public async Task<Result> RespondAsync(IMessageUpdate gatewayEvent, CancellationToken ct = default)
    {
        if (!gatewayEvent.ChannelID.IsDefined(out var channel))
            return Result.FromSuccess();
        
        if (!gatewayEvent.ID.IsDefined(out var message))
            return Result.FromSuccess();

        var key = new KeyHelpers.MessageCacheKey(channel, message);

        var cachedResult = await _cache.TryGetValueAsync<IMessage>(key, ct);
        
        if (!cachedResult.IsDefined(out var cached))
            return Result.FromSuccess();

        var updated = new Remora.Discord.API.Objects.Message
        (
         message,
         channel,
         cached.Author,
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
    
        await _cache.CacheAsync(key, updated, ct);

        return Result.FromSuccess();
    }
}