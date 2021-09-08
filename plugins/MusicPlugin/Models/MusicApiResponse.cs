using System;

namespace MusicPlugin
{
	public sealed record MusicApiResponse
	{
		public string Url { get; init; }
		public string Title { get; init; }
		public ulong Requester { get; init; }
		
		public TimeSpan Duration { get; init; }
	}
}