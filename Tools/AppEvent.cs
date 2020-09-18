using DSharpPlus.Entities;
using System;

namespace SilkBot.Tools
{
    public class AppEvent
    {
        public InfractionType EventType { get; set; }
        public ulong Id { get; set; }
        public DiscordGuild Guild { get; set; }
        public DateTime Expiration { get; set; }
        public string Reason { get; set; }
        
        public AppEvent()
        {
        }
    }
}
