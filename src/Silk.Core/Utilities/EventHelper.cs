using DSharpPlus;
using Silk.Core.EventHandlers;
using Silk.Core.EventHandlers.Guilds;
using Silk.Core.EventHandlers.MemberAdded;
using Silk.Core.EventHandlers.MemberRemoved;
using Silk.Core.EventHandlers.Messages;
using Silk.Core.EventHandlers.Messages.AutoMod;
using Silk.Core.EventHandlers.Reactions;

namespace Silk.Core.Utilities
{
    /// <summary>
    ///     Helper class for subscribing events to <see cref="DiscordShardedClient" />
    /// </summary>
    public sealed class EventHelper
    {
        public EventHelper(
            DiscordShardedClient client,
            CommandHandler commandHandler,
            MessageAddAntiInvite antiInvite,
            MessageRemovedHandler removeHandler,
            MemberGreetingService memberGreetingService,
            RoleAddedHandler staffCheck,
            RoleMenuReactionService roleMenu,
            GuildEventHandlers guildHandlers,
            MemberRemovedHandler memberRemovedHandler,
            ButtonHandlerService buttonHandler)
        {

            client.MessageCreated += commandHandler.Handle;
            client.MessageCreated += antiInvite.CheckForInvite;
            client.MessageDeleted += removeHandler.MessageRemoved;

            client.GuildMemberAdded += memberGreetingService.OnMemberAdded;
            client.GuildMemberRemoved += memberRemovedHandler.OnMemberRemoved;
            client.GuildMemberUpdated += staffCheck.CheckStaffRole;

            client.MessageReactionAdded += roleMenu.OnAdd;
            client.MessageReactionRemoved += roleMenu.OnRemove;


            client.GuildCreated += guildHandlers.OnGuildJoin;
            client.GuildAvailable += guildHandlers.OnGuildAvailable;
            client.GuildDownloadCompleted += guildHandlers.OnGuildDownload;

            client.ComponentInteractionCreated += buttonHandler.OnButtonPress;

        }
    }
}