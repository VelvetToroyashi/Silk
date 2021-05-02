using System;
using DSharpPlus;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using Microsoft.Extensions.Logging;
using Serilog.Extensions.Logging;
using Silk.Shared.Constants;

namespace Silk.Core.Discord.Utilities
{
    public static class DiscordConfigurations
    {
        public static DiscordConfiguration Discord { get; } = new()
        {
            Intents = FlagConstants.Intents,
            LogTimestampFormat = "h:mm:ss ff tt",
            MessageCacheSize = 1024,
            MinimumLogLevel = LogLevel.None,
            LoggerFactory = new SerilogLoggerFactory()
        };

        public static InteractivityConfiguration Interactivity { get; } = new()
        {
            PaginationBehaviour = PaginationBehaviour.WrapAround,
            PaginationDeletion = PaginationDeletion.DeleteMessage,
            PollBehaviour = PollBehaviour.DeleteEmojis,
            Timeout = TimeSpan.FromMinutes(1)
        };
    }
}