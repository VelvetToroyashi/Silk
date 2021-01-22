using System;

namespace Silk.Core.Tools
{
    public interface ITimedEvent
    {
        public Action<ITimedEvent> Callback { get; set; }
        public DateTime Expiration { get; set; }
    }
}