using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Humanizer;
using Mediator;
using Remora.Discord.API;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Caching;
using Remora.Discord.Caching.Services;
using Remora.Discord.Gateway.Responders;
using Remora.Rest.Core;
using Remora.Results;
using Silk.Data.MediatR.Guilds;
using Silk.Extensions;
using Silk.Extensions.Remora;
using Silk.Services.Data;
using Silk.Services.Interfaces;
using IMessage = Remora.Discord.API.Abstractions.Objects.IMessage;

namespace Silk.Responders.Message;

[ResponderGroup(ResponderGroup.Early)]
public class BulkMessageLoggerResponder : IResponder<IMessageDeleteBulk>
{
    private readonly IMediator               _mediator;
    private readonly CacheService            _cache;
    private readonly IDiscordRestChannelAPI  _channels;
    private readonly IChannelLoggingService  _logging;
    private readonly IDiscordRestAuditLogAPI _auditLogs;
    
    public BulkMessageLoggerResponder
    (
        IMediator               mediator,
        CacheService            cache,
        IDiscordRestChannelAPI  channels,
        IChannelLoggingService  logging,
        IDiscordRestAuditLogAPI auditLogs
    )
    {
        _cache     = cache;
        _channels  = channels;
        _logging   = logging;
        _mediator    = mediator;
        _auditLogs = auditLogs;
    }
    
    public async Task<Result> RespondAsync(IMessageDeleteBulk gatewayEvent, CancellationToken ct = default)
    {
        if (!gatewayEvent.GuildID.IsDefined(out var guildID))
            return Result.FromSuccess();
        
        var guildConfig = await _mediator.Send(new GetGuildConfig.Request(guildID), ct);
        
        if (!guildConfig.Logging.LogMessageDeletes)
            return Result.FromSuccess();

        //TODO: Listen for GUILD_AUDIT_LOG_CREATE and use that instead of fetching the audit log
        var auditLogs = await _auditLogs.GetGuildAuditLogAsync(guildID, actionType: AuditLogEvent.MessageBulkDelete, limit: 1, ct: ct);

        IUser? user = null;

        if (auditLogs.IsDefined(out var log))
            user = log.Users.FirstOrDefault();

        var channelResult = await _channels.GetChannelAsync(gatewayEvent.ChannelID, ct);
        
        if (!channelResult.IsDefined(out var channel))
            return Result.FromSuccess();

        IEmbed embed = new Embed
        {
            Title  = "Bulk Message Removal Detected",
            Colour = Color.Red,
            Author = new EmbedAuthor
            (
             user?.ToDiscordTag() ?? "Unknown#0000",
             IconUrl: (user is null ? CDN.GetDefaultUserAvatarUrl(0).Entity : (user.Avatar is null ? CDN.GetDefaultUserAvatarUrl(user) : CDN.GetUserAvatarUrl(user)).Entity).ToString()
            ),
            Fields = new EmbedField[]
            {
                new("User" , $"{(user is null ? "I don't have audit log permissions!" : $"**{user.ToDiscordTag()}**\n(`{user.ID}`)")}", true),
                new("Count", gatewayEvent.IDs.Count.ToString(), true),
                new("Channel", channel.Mention(), true),
                new("From", gatewayEvent.IDs.MinBy(x => x.Timestamp).Timestamp.ToTimestamp(TimestampFormat.LongDateTime), true),
                new("To", gatewayEvent.IDs.MaxBy(x => x.Timestamp).Timestamp.ToTimestamp(TimestampFormat.LongDateTime), true)
            }
        };

        var renderedMessage = await GenerateBulkDeleteDataAsync(gatewayEvent.ChannelID, gatewayEvent.IDs);

        return await _logging.LogAsync(guildConfig.Logging.UseWebhookLogging, guildConfig.Logging.MessageDeletes!, null, new[] { embed }, new[] { renderedMessage });
    }
    
    private async Task<FileData> GenerateBulkDeleteDataAsync(Snowflake channelID, IReadOnlyList<Snowflake> IDs)
    {
        var sb = new StringBuilder();

        for (var i = IDs.Count - 1; i >= 0; i--)
        {
            var ID  = IDs[i];
            var key = new KeyHelpers.MessageCacheKey(channelID, ID);

            if (!(await _cache.TryGetPreviousValueAsync<IMessage>(key)).IsDefined(out var message) && !(await _cache.TryGetValueAsync<IMessage>(key)).IsDefined(out message))
            {
                sb.AppendLine($"<Message not found> | {ID}");
                continue;
            }

            sb.AppendLine($"{message.Author.ToDiscordTag()} ({message.Author.ID}) at {message.ID.Timestamp:MM/dd/yyyy hh:mm} [{ID}]");

            if (message.MessageReference.IsDefined(out var reference))
            {
                var replyKey = new KeyHelpers.MessageCacheKey(channelID, reference.MessageID.Value);
                var replyResult = await _cache.TryGetPreviousValueAsync<IMessage>(replyKey);

                if (!replyResult.IsDefined(out var reply))
                {
                    sb.AppendLine($"<Reply not found> [{reference.MessageID}]");
                }
                else
                {
                    sb.Append($"➜ Replying to {reply.Author.ToDiscordTag()} {reply.Author.ID} [{reply.ID}]: ");
                    sb.AppendLine($"{(string.IsNullOrEmpty(message.Content) ? "Message did not contain content" : reply.Content.Truncate(120, "[...]"))}");
                }
            }
            
            sb.AppendLine(string.IsNullOrEmpty(message.Content) ? "<Message did not contain content>" : message.Content);
            
            if (message.Attachments.Any())
                sb.AppendLine($"<Message contains {message.Attachments.Count} attachments>");

            if (message.Embeds.Any())
                sb.AppendLine($"<Message contains {message.Embeds.Count} embeds>");

            sb.AppendLine();
        }

        var stream = sb.ToString().AsStream();

        return new($"{DateTime.UtcNow:MM-dd-yyyy_HH-mm-ss}_BulkDelete.txt", stream, null!);
    }
}