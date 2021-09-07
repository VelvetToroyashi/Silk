using System;
using DSharpPlus.Entities;
using Silk.Core.Types;

namespace Silk.Core.Services.Bot.Music
{
	public sealed record MusicTrack 
	{
		public string Title { get; init; }
		public string Url { get; init; }
		public TimeSpan Duration { get; init ; }
		
		public DiscordUser Requester { get; init; }
		public LazyLoadHttpStream Stream { get; init; }
	}
}