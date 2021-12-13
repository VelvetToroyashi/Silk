using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OneOf;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Abstractions.Results;
using Remora.Discord.API.Objects;
using Remora.Rest.Core;
using Remora.Rest.Results;
using Remora.Results;
using Silk.Core.Data.Entities;

namespace Silk.Core.Services.Bot;

public class ChannelLoggingService
{
    private readonly IDiscordRestChannelAPI _channels;
    private readonly IDiscordRestWebhookAPI _webhooks;
 
    private readonly ILogger<ChannelLoggingService> _logger;
    
    public ChannelLoggingService
    (
        IDiscordRestChannelAPI channels, 
        IDiscordRestWebhookAPI webhooks,
        ILogger<ChannelLoggingService> logger
    )
    {
        _channels = channels;
        _webhooks = webhooks;
        _logger   = logger;
    }

    /// <summary>
    /// Logs to the configured channel.
    /// </summary>
    /// <param name="useWebhook">Whether to log the message via a webhook.</param>
    /// <param name="loggingData">The logging configuration to use.</param>
    /// <param name="content">The content to log.</param>
    public virtual Task<Result> LogAsync(bool useWebhook, LoggingChannelEntity loggingData, OneOf<string, IEmbed> content)
    {
        if (useWebhook)
            return LogWebhookAsync(loggingData, content);
        else 
            return LogChannelAsync(loggingData, content);
    }
    
    /// <summary>
    /// Logs to a configured channel.
    /// </summary>
    /// <param name="loggingData">The logging configuration to use.</param>
    /// <param name="content">The content to log.</param>
    protected virtual async Task<Result> LogChannelAsync(LoggingChannelEntity loggingData, OneOf<string, IEmbed> content)
    {
        var channel = await _channels.GetChannelAsync(loggingData.ChannelID);
        
        if (!channel.IsSuccess)
        {
            _logger.LogError($"Failed to get channel {loggingData.ChannelID}");
            return Result.FromError(channel.Error);
        }
        
        var result = await _channels
           .CreateMessageAsync(
                               loggingData.ChannelID, 
                               content.IsT0 
                                   ? content.AsT0 
                                   : default(Optional<string>), 
                               embeds: content.IsT1 
                                   ? new[] {content.AsT1 }
                                   : default(Optional<IReadOnlyList<IEmbed>>)
                               );


        if (!result.IsSuccess)
        {
            LogReadableError(result.Error, loggingData.GuildID);
            return Result.FromError(result.Error); 
        }
        
        return  Result.FromSuccess();
    }
    
    /// <summary>
    /// Logs to a channel via a configured webhook.
    /// </summary>
    /// <param name="loggingData">The logging configuration to use.</param>
    /// <param name="content">The content to log.</param>
    protected virtual async Task<Result> LogWebhookAsync(LoggingChannelEntity loggingData, OneOf<string, IEmbed> content)
    {
        var result = await _webhooks.ExecuteWebhookAsync(loggingData.WebhookID, loggingData.WebhookToken, true,
                                                         content.IsT0 
                                                             ? content.AsT0 
                                                             : default(Optional<string>), 
                                                         embeds: content.IsT1 
                                                             ? new[] {content.AsT1 }
                                                             : default(Optional<IReadOnlyList<IEmbed>>),
                                                         username: "Silk! Logging");

        if (!result.IsSuccess)
        {
            LogReadableError(result.Error, loggingData.GuildID);
            return Result.FromError(result.Error); 
        }
        
        return Result.FromSuccess();
    }
    
    /// <summary>
    /// Logs an error when logging to a channel fails.
    /// </summary>
    /// <param name="error">The error that occured.</param>
    /// <param name="guildID">The ID of the guild the error occured on.</param>
    /// <exception cref="ArgumentException">The error was not a REST error.</exception>
    private void LogReadableError(IResultError error, Snowflake guildID)
    {
        if (error is not RestResultError<RestError> re)
            throw new ArgumentException($"Expected {nameof(RestResultError<RestError>)} but got {error.GetType().Name}", nameof(error));

        switch (re.Error.Code)
        {
            case DiscordError.MissingAccess:
                _logger.LogError("Configured logging channel for {Guild} exists, but is locked.", guildID);
                break;
            case DiscordError.UnknownChannel:
                _logger.LogError("Configured logging channel for {Guild} does not exist.", guildID);
                break;
            case DiscordError.UnknownWebhook:
                _logger.LogError("Configured logging channel for {Guild} has webhook logging, but the webhook is missing.", guildID);
                break;
            case DiscordError.InvalidWebhookToken:
                _logger.LogError("Configured logging channel for {Guild} has webhook logging, but the webhook token is invalid.", guildID);
                break;
            default:
                _logger.LogError("Something catostrophic happened while logging to {Guild}.", guildID);
                break;
        }
    }
}