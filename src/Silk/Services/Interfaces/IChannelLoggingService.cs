using System.Threading.Tasks;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Results;
using Silk.Data.Entities;

namespace Silk.Services.Interfaces;

public interface IChannelLoggingService
{
    /// <summary>
    /// Logs to the configured channel.
    /// </summary>
    /// <param name="useWebhook">Whether to log the message via a webhook.</param>
    /// <param name="loggingData">The logging configuration to use.</param>
    /// <param name="content">The content to log.</param>
    Task<Result> LogAsync(bool useWebhook, LoggingChannelEntity loggingData, string? contentString = null, IEmbed? embedContent = null);

    /// <inheritdoc cref="LogAsync(bool,Silk.Data.Entities.LoggingChannelEntity,string?,Remora.Discord.API.Abstractions.Objects.IEmbed?)"/>
    Task<Result> LogAsync(bool useWebhook, LoggingChannelEntity loggingData, string? contentString = null, IEmbed[]? embed = null, FileData[]? files = null);
}