using System;
using DSharpPlus;
using MediatR;
using Silk.Core.Discord.EventHandlers;
using Silk.Core.Discord.EventHandlers.MemberAdded;
using Silk.Core.Discord.EventHandlers.Notifications;
using Silk.Extensions;

namespace Silk.Core.Discord.Utilities.Bot
{
    public static class EventHelper
    {
        public static void SubscribeToEvents(DiscordShardedClient shardClient, IServiceProvider services)
        {
            //Client.MessageCreated += _services.Get<AutoModInviteHandler>().MessageAddInvites; // I'll fix AutoMod eventually™ ~Velvet, May 3rd, 2021. //
            var mediator = services.Get<IMediator>();

            // Direct Dispatch //
            shardClient.MessageDeleted += services.Get<MessageRemovedHandler>()!.MessageRemoved;

            shardClient.GuildMemberAdded += services.Get<MemberAddedHandler>()!.OnMemberAdded;
            shardClient.GuildCreated += services.Get<GuildAddedHandler>()!.SendThankYouMessage;
            shardClient.GuildAvailable += services.Get<GuildAddedHandler>()!.OnGuildAvailable;
            shardClient.GuildCreated += services.Get<GuildAddedHandler>()!.OnGuildAvailable;
            shardClient.GuildDownloadCompleted += services.Get<GuildAddedHandler>()!.OnGuildDownloadComplete;
            shardClient.GuildMemberUpdated += services.Get<RoleAddedHandler>()!.CheckStaffRole;

            // MediatR Dispatch //

            shardClient.GuildDownloadCompleted += async (cl, __) =>
                cl.MessageCreated += async (c, e) => { mediator.Publish(new MessageCreated(c, e.Message!)); };

            shardClient.MessageUpdated += async (c, e) => { mediator.Publish(new MessageEdited(c, e)); };
        }
    }
}