using Microsoft.Extensions.Logging;

namespace Silk.Shared.Constants
{
    public static class EventIds
    {
        public static EventId
            Core         = new(0, "Core"),
            Service      = new(1, "Service"),
            AutoMod      = new(2, "AutoMod"),
            Database     = new(3, "Database"),
            EventHandler = new(4, "EventHandler");
    }
}