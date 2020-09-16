using DSharpPlus.Entities;
using System;

namespace SilkBot.Tools
{
    public class TimedRestrictionAction : IDisposable
    {
        private bool disposedValue;

        public RestrictionActionReason ActionReason { get; set; }
        public ulong Id { get; set; }
        public DiscordGuild Guild { get; set; }
        public DateTime Expiration { get; set; }
        public string Reason { get; set; }
        
        public TimedRestrictionAction()
        {
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing){}
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
