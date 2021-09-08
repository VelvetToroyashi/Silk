using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Entities;
using YoutubeExplode.Playlists;
using YoutubeExplode.Videos;

namespace MusicPlugin.Utilities
{
	public class VideoIdConverter : IArgumentConverter<VideoId>
	{
		public async Task<Optional<VideoId>> ConvertAsync(string value, CommandContext ctx)
		{
			var id = VideoId.TryParse(value);

			if (id is null)
				return Optional.FromNoValue<VideoId>();

			return Optional.FromValue(id.Value);
		}
	}
	
	public class PlaylistIdConverter : IArgumentConverter<PlaylistId>
	{
		public async Task<Optional<PlaylistId>> ConvertAsync(string value, CommandContext ctx)
		{
			var id = PlaylistId.TryParse(value);

			if (id is null)
				return Optional.FromNoValue<PlaylistId>();

			return Optional.FromValue(id.Value);
		}
	}
}