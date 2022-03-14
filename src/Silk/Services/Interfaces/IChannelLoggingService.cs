using System.Threading.Tasks;
using Remora.Discord.API.Abstractions.Objects;
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
    Task<Result> LogAsync(bool useWebhook, LoggingChannelEntity loggingData, string? contentString, IEmbed? embedContent);
}