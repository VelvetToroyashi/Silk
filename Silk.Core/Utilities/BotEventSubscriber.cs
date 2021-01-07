using System;
using System.Linq;
using System.Threading;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using Microsoft.Extensions.Logging;
using Silk.Core.AutoMod;
using Silk.Core.Tools.EventHelpers;
using Silk.Extensions;

namespace Silk.Core.Utilities
{
    public class BotEventSubscriber
    {
        private readonly DiscordShardedClient _client;
        private readonly ILogger<BotEventSubscriber> _logger;
        private IServiceProvider _services;

        public BotEventSubscriber(DiscordShardedClient client, ILogger<BotEventSubscriber> logger,
            IServiceProvider services) => (_logger, _client, _services) = (logger, client, services);

        public void SubscribeToEvents()
        {
            _logger.LogInformation("Subscribing to events.");

            _client.MessageCreated += _services.Get<MessageAddedHandler>().Commands;
            _logger.LogTrace("Subscribed to:" + " MessageAddedHelper/Commands".PadLeft(40));

            _client.MessageCreated += _services.Get<MessageAddedHandler>().Tickets;
            _logger.LogTrace("Subscribed to:" + " MessageAddedHelper/Tickets".PadLeft(40));

            _client.MessageCreated += _services.Get<AutoModMessageHandler>().CheckForInvites;
            _logger.LogTrace("Subscribed to:" + " AutoMod/CheckForInvites".PadLeft(40));

            _client.MessageDeleted += _services.Get<MessageRemovedHandler>().OnRemoved;
            _logger.LogTrace("Subscribed to:" + " MessageRemovedHelper/MessageRemoved".PadLeft(40));

            _client.GuildMemberRemoved += _services.Get<MemberRemovedHandler>().OnMemberRemoved;
            _logger.LogTrace("Subscribed to:" + " MemberRemovedHelper/MemberRemoved".PadLeft(40));

            _client.GuildCreated += _services.Get<GuildAddedHandler>().OnGuildJoin;
            _logger.LogTrace("Subscribed to:" + " GuildAddedHelper/GuildAdded".PadLeft(40));

            _client.GuildAvailable += _services.Get<GuildAddedHandler>().OnGuildAvailable;
            _logger.LogTrace("Subscribed to:" + " GuildAddedHelper/GuildAvailable".PadLeft(40));

            _client.GuildDownloadCompleted += _services.Get<GuildAddedHandler>().OnGuildDownloadComplete;
            _logger.LogTrace("Subscribed to:" + "  GuildAddedHelper/GuildDownloadComplete");

            _client.GuildMemberUpdated += _services.Get<RoleAddedHandler>().CheckStaffRole;
            _logger.LogTrace("Subscribed to:" + " RoleAddedHelper/CheckForStaffRole".PadLeft(40));

            _logger.LogInformation("Subscribed to all events!");
        }
    }
}