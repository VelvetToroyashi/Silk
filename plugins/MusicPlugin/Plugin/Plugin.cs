using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using MusicPlugin.Utilities;

namespace MusicPlugin
{
	public class Requisite : YumeChan.PluginBase.Plugin
	{
		public override string DisplayName { get; }

		private readonly DiscordClient _client;
		public Requisite(DiscordClient client) => _client = client;
		
		public override async Task LoadAsync()
		{
			await base.LoadAsync();

			var cnext = _client.GetCommandsNext();
			
			cnext.RegisterConverter(new VideoIdConverter());
			cnext.RegisterConverter(new PlaylistIdConverter());
		}

		public override async Task UnloadAsync()
		{
			await base.UnloadAsync();
			
			var cnext = _client.GetCommandsNext();
			
			cnext.UnregisterConverter<VideoIdConverter>();
			cnext.UnregisterConverter<PlaylistIdConverter>();
		}
	}
}