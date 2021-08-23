using DSharpPlus;
using Silk.Core.AutoMod;
using Silk.Core.EventHandlers;
using Silk.Core.EventHandlers.Guilds;
using Silk.Core.EventHandlers.MemberAdded;
using Silk.Core.EventHandlers.MemberRemoved;
using Silk.Core.EventHandlers.Messages;
using Silk.Core.EventHandlers.Messages.AutoMod;

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
			GuildEventHandler guildHandler,
			MemberRemovedHandler memberRemovedHandler,
			AutoModMuteApplier _)
		{

			client.MessageCreated += commandHandler.Handle;
			client.MessageCreated += antiInvite.CheckForInvite;
			client.MessageDeleted += removeHandler.MessageRemoved;

			client.GuildMemberAdded += memberGreetingService.OnMemberAdded;
			client.GuildMemberRemoved += memberRemovedHandler.OnMemberRemoved;
			client.GuildMemberUpdated += staffCheck.CheckStaffRole;

			client.GuildCreated += guildHandler.OnGuildJoin;
			client.GuildAvailable += guildHandler.OnGuildAvailable;
			client.GuildDownloadCompleted += guildHandler.OnGuildDownload;
		}
	}
}