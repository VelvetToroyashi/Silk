using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.EventArgs;

namespace Silk.Core.EventHandlers.Guilds
{
	public sealed class GuildEventHandler
	{
		private readonly GuildCacher _guildHandler;
		public GuildEventHandler(DiscordClient client, GuildCacher guildHandler)
		{
			client.GuildCreated += OnGuildJoin;
			client.GuildAvailable += OnGuildAvailable;
			
			_guildHandler = guildHandler;
		}

		public async Task OnGuildJoin(DiscordClient client, GuildCreateEventArgs args)
		{
			_ =  _guildHandler.JoinedGuild(args);
		}

		public async Task OnGuildAvailable(DiscordClient client, GuildCreateEventArgs args)
		{
			_ = _guildHandler.CacheGuildAsync(args.Guild);
		}
	}
}