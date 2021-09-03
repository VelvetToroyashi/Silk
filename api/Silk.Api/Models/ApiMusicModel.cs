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
		public bool Played { get; init; }
		
		[JsonIgnore]
		public int Index { get; init; }
		
		[JsonIgnore]
		public TimeSpan Duration { get; init; }
	}
}