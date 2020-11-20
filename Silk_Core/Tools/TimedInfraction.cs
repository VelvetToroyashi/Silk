using System;

namespace SilkBot.Tools
{
    public class TimedInfraction : ITimedEvent
    {
        public ulong Id { get; }
        public ulong Guild { get; }
        public DateTime Expiration { get; set; }
        public string Reason { get; }
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
