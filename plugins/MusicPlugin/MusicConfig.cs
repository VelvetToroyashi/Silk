namespace MusicPlugin
{
	public class MusicConfig
	{
		public string ApiKey { get; init; }
		public string FfmpegPath { get; init; }
		public ulong[] MusicGuilds { get; init; }
		public string MusicApiUrl => "https://localhost:5001/api/v1";
	}
}