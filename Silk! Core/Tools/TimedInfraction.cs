using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using System;

namespace SilkBot.Tools
{
    public class TimedInfraction : ITimedEvent
    {
        public ulong Id { get; set; }
        public ulong Guild { get; set; }
        public DateTime Expiration { get; set; }
        public string Reason { get; set; }
        public Action<ITimedEvent> Callback { get; set; }

        public TimedInfraction(ulong id, ulong guild, DateTime expiration, string reason, Action<ITimedEvent> callback)
        {
            Id = id;
            Guild = guild;
            Expiration = expiration;
            Reason = reason;
            Callback = callback;
        }
    }
}
