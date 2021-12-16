using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Abstractions.Results;
using Remora.Discord.API.Objects;
using Remora.Rest.Core;
using Remora.Rest.Results;
using Remora.Results;
using Silk.Data.Entities;

namespace Silk.Services.Bot;

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
    public virtual Task<Result> LogAsync(bool useWebhook, LoggingChannelEntity loggingData, string? contentString, IEmbed? embedContent)
    {
        if (useWebhook)
            return LogWebhookAsync(loggingData, contentString, embedContent);
        else 
            return LogChannelAsync(loggingData, contentString, embedContent);
    }
    
    /// <summary>
    /// Logs to a configured channel.
    /// </summary>
    /// <param name="loggingData">The logging configuration to use.</param>
    /// <param name="content">The content to log.</param>
    protected virtual async Task<Result> LogChannelAsync(LoggingChannelEntity loggingData, string? stringContent, IEmbed? embedContent)
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
                               stringContent, 
                               embeds: embedContent is null 
                                   ? default(Optional<IReadOnlyList<IEmbed>>) : new[] { embedContent }
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
    protected virtual async Task<Result> LogWebhookAsync(LoggingChannelEntity loggingData, string? stringContent, IEmbed? embedContent)
    {
        var result = await _webhooks.ExecuteWebhookAsync(loggingData.WebhookID, loggingData.WebhookToken, true,
                                                         stringContent ?? default(Optional<string>), 
                                                         embeds: embedContent is not null
                                                             ? new[] { embedContent }
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