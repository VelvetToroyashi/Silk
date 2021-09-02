using System;
using System.Text.Json.Serialization;

namespace Silk.Api.Models
{
	public sealed record ApiMusicModel
	{
		public string Url { get; init; }
		public string Title { get; init; }
		public ulong Requester { get; init; }

		public string Timestamp => Duration.ToString();
		
		[JsonIgnore]
		public TimeSpan Duration { get; init; }
	}
}