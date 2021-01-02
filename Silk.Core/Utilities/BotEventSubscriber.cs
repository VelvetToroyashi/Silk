using DSharpPlus;
using Microsoft.Extensions.Logging;
using Silk.Core.Tools.EventHelpers;

namespace Silk.Core.Utilities
{
    public class BotEventSubscriber
    {
        public BotEventSubscriber(DiscordShardedClient client, ILogger<BotEventSubscriber> logger,
            MessageAddedHelper messageAHelper, MessageRemovedHelper messageRHelper,
            GuildAddedHelper guildAddedHelper, /* GuildRemovedHelper guildRemovedHelper,*/
            MemberRemovedHelper memberRemovedHelper,
            RoleAddedHelper roleAddedHelper, RoleRemovedHelper roleRemovedHelper)
        {

            logger.LogInformation("Subscribing to events. Some features may be unavailable during this time.");
            client.MessageCreated += messageAHelper.Commands;
            logger.LogTrace("Subscribed to:" +" MessageAddedHelper/Commands".PadLeft(40));
            client.MessageCreated += messageAHelper.Tickets;
            logger.LogTrace("Subscribed to:" +" MessageAddedHelper/Tickets".PadLeft(40));
            client.MessageDeleted += messageRHelper.OnRemoved;
            logger.LogTrace("Subscribed to:" +" MessageRemovedHelper/MessageRemoved".PadLeft(40));
            client.GuildMemberRemoved += memberRemovedHelper.OnMemberRemoved;
            logger.LogTrace("Subscribed to:" +" MemberRemovedHelper/MemberRemoved".PadLeft(40));
            client.GuildCreated += guildAddedHelper.OnGuildJoin;
            logger.LogTrace("Subscribed to:" +" GuildAddedHelper/GuildAdded".PadLeft(40));
            client.GuildAvailable += guildAddedHelper.OnGuildAvailable;
            logger.LogTrace("Subscribed to:" +" GuildAddedHelper/GuildAvailable".PadLeft(40));
            client.GuildDownloadCompleted += guildAddedHelper.OnGuildDownloadComplete;
            logger.LogTrace("Subscribed to:" +"  GuildAddedHelper/GuildDownloadComplete");
            client.GuildMemberUpdated += roleAddedHelper.CheckStaffRole;
            logger.LogTrace("Subscribed to:" +" RoleAddedHelper/CheckForStaffRole".PadLeft(40));
            client.GuildMemberUpdated += roleRemovedHelper.CheckStaffRoles;
            logger.LogTrace("Subscribed to:" +" RoleRemovedHelper/CheckStaffRoles".PadLeft(40));
            logger.LogInformation("Subscribed to all events!");
        }
        
    }
}