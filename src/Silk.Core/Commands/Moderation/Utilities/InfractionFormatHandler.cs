namespace Silk.Core.Commands.Moderation.Utilities
{
    public static class InfractionFormatHandler
    {
        public static string ParseInfractionFormat(string action, string duration, string mention, string reason, string infractionFormat)
        {
            return infractionFormat.Replace("{action}", action)
                .Replace("{duration}", duration)
                .Replace("{mention}", mention)
                .Replace("{reason}",reason );
        }
    }
}