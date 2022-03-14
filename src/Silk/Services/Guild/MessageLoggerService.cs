using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Remora.Discord.API;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Caching;
using Remora.Discord.Caching.Services;
using Remora.Rest.Core;
using Remora.Results;
using Silk.Data.Entities;
using Silk.Extensions;
using Silk.Extensions.Remora;
using Silk.Services.Bot;
using Silk.Services.Data;
using Silk.Services.Interfaces;

namespace Silk.Services.Guild;

public class MessageLoggerService
{
    private readonly HttpClient                 _http;
    private readonly CacheService               _cache;
    private readonly GuildConfigCacheService    _config;
    private readonly IChannelLoggingService     _channelLogger;
    private readonly ExemptionEvaluationService _exemptions;
    
    public MessageLoggerService(IHttpClientFactory httpFactory, CacheService cache, GuildConfigCacheService config, IChannelLoggingService channelLogger, ExemptionEvaluationService exemptions)
    {
        _http          = httpFactory.CreateClient();
        _cache         = cache;
        _config        = config;
        _channelLogger = channelLogger;
        _exemptions    = exemptions;
    }

    public async Task<Result> LogEditAsync(IPartialMessage message)
    {
        if (!message.Author.IsDefined(out var author))
            return Result.FromSuccess();
        
        if (author.IsBot.IsDefined(out var bot) && bot)
            return Result.FromSuccess();

        if (!message.GuildID.IsDefined(out var guildID))
            return Result.FromSuccess();
        
        if (!message.ChannelID.IsDefined(out var channelID))
            return Result.FromSuccess();
        
        var config = await _config.GetModConfigAsync(guildID);
        
        if (!config.Logging.LogMessageEdits)
            return Result.FromSuccess();

        var exemptionResult = await _exemptions.EvaluateExemptionAsync(ExemptionCoverage.EditLogging, guildID, author.ID, channelID);

        if (!exemptionResult.IsDefined(out var exempt))
            return Result.FromError(exemptionResult.Error!);
        
        if (exempt)
            return Result.FromSuccess();

        _ = _cache.TryGetValue<IMessage>(KeyHelpers.CreateMessageCacheKey(channelID, message.ID.Value), out var previousMessage);
        
        var beforeContent = previousMessage is null 
            ? "It doesn't seem I was around when this happened. Sorry." 
            : string.IsNullOrEmpty(previousMessage.Content) 
            ? "Message did not contain content!"
            : $"> {previousMessage.Content.Replace("\n", "\n> ")}";

        var embed = new Embed
        {
            Title = "Message Edited",
            Thumbnail = new EmbedThumbnail(author.Avatar is null
                                       ? CDN.GetDefaultUserAvatarUrl(author, imageSize: 256).Entity.ToString()
                                       : CDN.GetUserAvatarUrl(author, imageSize: 256).Entity.ToString()
                                  ),
            Description = $"**Content Before:** \n{beforeContent}\n\n" +
                          $"After: \n{(message.Content.IsDefined(out var content) ? $"> {content.Replace("\n", "\n> ")}" : "Message did not contain content!")}",
            
            Colour = Color.DarkOrange,
            
            Fields = new EmbedField[]
            {
                new("Channel", $"<#{channelID}>", true),
                new("Thread", message.Thread.IsDefined(out var thread) ? $"<#{thread.ID}>" : "None", true),
                new("\u200b", "\u200b", true),
                new("Sent At", message.ID.Value.Timestamp.ToTimestamp(), true),
                new("Edited At", message.EditedTimestamp.Value.Value.ToTimestamp(), true),
                new("\u200b", "\u200b", true),
                new("Message ID", $"[{message.ID.Value}](https://discordapp.com/channels/{guildID}/{channelID}/{message.ID.Value})", true),
                new("User ID", $"[{author.ID}](https://discordapp.com/users/{author.ID})", true)
            }
        };

        return await _channelLogger.LogAsync(config.Logging.UseWebhookLogging, config.Logging.MessageEdits, null, embed);
    }

    public async Task<Result> LogDeleteAsync(IMessageDelete message)
    {
        var key = KeyHelpers.CreateMessageCacheKey(message.ChannelID, message.ID);
        
        if (!_cache.TryGetValue<IMessage>(key, out var cachedMessage))
            return Result.FromSuccess();

        if (cachedMessage.Author.IsBot.IsDefined(out var bot) && bot)
            return Result.FromSuccess();
        
        if (!message.GuildID.IsDefined(out var guildID))
            return Result.FromSuccess();
        
        var config = await _config.GetModConfigAsync(guildID);
        
        if (!config.Logging.LogMessageDeletes)
            return Result.FromSuccess();
        
        var exemptionResult = await _exemptions.EvaluateExemptionAsync(ExemptionCoverage.DeleteLogging, guildID, cachedMessage.Author.ID, message.ChannelID);
        
        if (!exemptionResult.IsDefined(out var exempt))
            return Result.FromError(exemptionResult.Error!);
  
        if (exempt)
            return Result.FromSuccess();

        var embeds = new List<IEmbed>();

        var mainEmbed = new Embed
        {
            Title = "Message Deleted",
            Thumbnail = new EmbedThumbnail
                (
                 cachedMessage.Author.Avatar is null
                     ? CDN.GetDefaultUserAvatarUrl(cachedMessage.Author, imageSize: 256).Entity.ToString()
                     : CDN.GetUserAvatarUrl(cachedMessage.Author, imageSize: 256).Entity.ToString()
                ),

            Description = $"**Content:** \n{(string.IsNullOrEmpty(cachedMessage.Content) ? "Message did not contain content!" : $"> {cachedMessage.Content.Replace("\n", "\n> ")}")}",

            Colour = Color.Red,

            Fields = new EmbedField[]
            {
                new("Channel", $"<#{message.ChannelID}>", true),
                new("Thread", cachedMessage.Thread.IsDefined(out var thread) ? $"<#{thread.ID}>" : "None", true),
                new("\u200b", "\u200b", true),
                new("Sent At", message.ID.Timestamp.ToTimestamp(), true),
                new("Edited At", cachedMessage.EditedTimestamp.HasValue ? cachedMessage.EditedTimestamp.Value.ToTimestamp() : "Never", true),
                new("\u200b", "\u200b", true),
                new("Message ID", $"[{message.ID.Value}](https://discordapp.com/channels/{guildID}/{message.ChannelID}/{message.ID.Value})", true),
                new("User ID", $"[{cachedMessage.Author.ID}](https://discordapp.com/users/{cachedMessage.Author.ID})", true)
            }
        };
        
        embeds.Add(mainEmbed);

        var files = new List<FileData>();

        if (cachedMessage.Attachments.Any())
        {
            for (var i = 0; i < cachedMessage.Attachments.Count; i++)
            {
                var attachment = cachedMessage.Attachments[i];
                if (!attachment.ContentType.IsDefined(out var ct))
                    continue;

                var url = new Uri($"https://cdn.discordapp.com/attachments/{message.ChannelID}/{message.ID}/{attachment.Filename}.{ct.Split('/').Last()}");

                var stream = await _http.GetStreamAsync(url);

                var embed = new Embed
                {
                    Colour = Color.DarkRed,
                    Title = "Attachment Deleted",
                    Description = $"**Attachment:** {attachment.Filename}",
                    Image = new EmbedImage($"attachment://attachment{i}.{ct.Split('/').Last()}")
                };
                
                files.Add(new FileData($"attachment{i}.{ct.Split('/').Last()}", stream));
            }
        }

        return await _channelLogger.LogAsync(config.Logging.UseWebhookLogging, config.Logging.MessageDeletes!, null, embeds.ToArray(), files.ToArray());
    }

}