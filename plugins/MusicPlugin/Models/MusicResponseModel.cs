using System;

namespace MusicPlugin.Models
{
	public sealed record MusicResponseModel
	{
		public string Url { get; init; }
		public string Title { get; init; }
		public ulong Requester { get; init; }
		public TimeSpan Duration { get; init; }
	}
}