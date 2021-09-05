namespace MusicPlugin
{
	public sealed class MusicPluginConfig
	{
		public string ApiKey { get; init; }
		public string FfmpegPath { get; init; }
		public ulong[] MusicGuilds { get; init; }
		public string MusicApiUrl { get; init; } = "https://silk.velvethepanda.dev/";
	}
}