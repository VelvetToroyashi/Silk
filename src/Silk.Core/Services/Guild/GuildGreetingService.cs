using System.Collections.Generic;
using MediatR;
using Microsoft.Extensions.Logging;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Rest.Core;

namespace Silk.Core.Services.Server
{
    public class GuildGreetingService
    {
        // Member Id ➜ Guild Id
        private readonly Dictionary<Snowflake, Snowflake> _membersToGreet = new();

        private readonly ILogger<GuildGreetingService> _logger;
        private readonly IMediator                     _mediator;
        private readonly IDiscordRestChannelAPI        _channelApi;
    }
}