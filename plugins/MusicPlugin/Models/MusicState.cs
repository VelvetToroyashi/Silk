using DSharpPlus.Entities;
using DSharpPlus.VoiceNext;

namespace MusicPlugin.Models
{
	public sealed class MusicState
	{
		public DiscordChannel Voice { get; set; }
		public DiscordChannel Commands { get; set; }
			
		public VoiceNextConnection VNext { get; set; }
	}
}