using System;
using System.Collections.Generic;
using System.Linq;
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
using Silk.Services.Interfaces;
using OneOf;

namespace Silk.Services.Bot;

public class ChannelLoggingService : IChannelLoggingService
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
            return LogWebhookAsync(loggingData, contentString, embedContent is null ? null : new[] {embedContent}, null);
        else 
            return LogChannelAsync(loggingData, contentString, embedContent is null ? null : new[] {embedContent}, null);
    }

    public virtual Task<Result> LogAsync(bool useWebhook, LoggingChannelEntity loggingData, string? contentString = null, IEmbed[]? embed = null, FileData[]? files = null)
    {
        if (useWebhook)
            return LogWebhookAsync(loggingData, contentString, embed, files);
        else 
            return LogChannelAsync(loggingData, contentString, embed, files);
    }
    
    protected virtual async Task<Result> LogChannelAsync(LoggingChannelEntity loggingData, string? stringContent, IEmbed[]? embedContent, FileData[]? fileData)
    {
        _logger.LogTrace("[REST] Attempting to log to {Channel} in {Guild}", loggingData.ChannelID, loggingData.GuildID);
        
        _logger.LogDebug("Preparring to log {EmbedCount} embeds with {FileCount} files with{Without} content", embedContent?.Length ?? 0, fileData?.Length ?? 0, stringContent is null ? "out" : null);

        
        var channel = await _channels.GetChannelAsync(loggingData.ChannelID);
        
        if (!channel.IsSuccess)
        {
            _logger.LogError($"Failed to get channel {loggingData.ChannelID}");
            return Result.FromError(channel.Error);
        }
        
        var result = await _channels
           .CreateMessageAsync(
                               loggingData.ChannelID, 
                               stringContent                                                                       ?? default(Optional<string>), 
                               embeds: embedContent                                                                ?? default(Optional<IReadOnlyList<IEmbed>>),
                               attachments: fileData?.Select(OneOf<FileData, IPartialAttachment>.FromT0).ToArray() ?? default(Optional<IReadOnlyList<OneOf<FileData, IPartialAttachment>>>)
                              );


        if (!result.IsSuccess)
        {
            LogReadableError(result.Error, loggingData.ChannelID, loggingData.GuildID);
            return Result.FromError(result.Error); 
        }
        
        return  Result.FromSuccess();
    }
    
    /// <summary>
    /// Logs to a channel via a configured webhook.
    /// </summary>
    /// <param name="loggingData">The logging configuration to use.</param>
    /// <param name="content">The content to log.</param>
    protected virtual async Task<Result> LogWebhookAsync(LoggingChannelEntity loggingData, string? stringContent, IEmbed[]? embedContent, FileData[]? fileData = null)
    {
        _logger.LogTrace("[WEBHOOK] Attempting to log to {Channel} in {Guild}", loggingData.ChannelID, loggingData.GuildID);
        
        _logger.LogDebug("Preparring to log {EmbedCount} embeds with {FileCount} files with{Without} content", embedContent?.Length ?? 0, fileData?.Length ?? 0, stringContent is null ? "out" : null);
        
        var result = await _webhooks.ExecuteWebhookAsync(loggingData.WebhookID,
                                                         loggingData.WebhookToken,
                                                         true,
                                                         stringContent                                                                       ?? default(Optional<string>), 
                                                         embeds: embedContent                                                                ?? default(Optional<IReadOnlyList<IEmbed>>),
                                                         attachments: fileData?.Select(OneOf<FileData, IPartialAttachment>.FromT0).ToArray() ?? default(Optional<IReadOnlyList<OneOf<FileData, IPartialAttachment>>>),
                                                         username: "Silk! Logging");

        if (!result.IsSuccess)
        {
            LogReadableError(result.Error, loggingData.ChannelID, loggingData.GuildID);
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
    private void LogReadableError(IResultError error, Snowflake channelID, Snowflake guildID)
    {
        if (error is not RestResultError<RestError> re)
            throw new ArgumentException($"Expected {nameof(RestResultError<RestError>)} but got {error.GetType().Name}", nameof(error));

        switch (re.Error.Code)
        {
            case DiscordError.MissingAccess:
                _logger.LogError("Configured logging channel for {Guild}➜{Channel} exists, but is locked.", guildID, channelID);
                break;
            case DiscordError.UnknownChannel:
                _logger.LogError("Configured logging channel for {Guild}➜{Channel} does not exist.", guildID, channelID);
                break;
            case DiscordError.UnknownWebhook:
                _logger.LogError("Configured logging channel for {Guild}➜{Channel} has webhook logging, but the webhook is missing.", guildID, channelID);
                break;
            case DiscordError.InvalidWebhookToken:
                _logger.LogError("Configured logging channel for {Guild}➜{Channel} has webhook logging, but the webhook token is invalid.", guildID, channelID);
                break;
            default:
                _logger.LogError("Something catostrophic happened while logging to {Guild}➜{Channel}.", guildID, channelID);
                break;
        }
    }
}