namespace Silk.Shared.Constants
{
    public static class StringConstants
    {
        public const string DefaultCommandPrefix = "s!";
        public const string Version = "2.2.11";
        public const string LogFormat = "[{@t:h:mm:ss ff tt}] [{@l:u3}]{#if EventId is not null} [{EventId.Name}]{#end} [{Substring(SourceContext, LastIndexOf(SourceContext, '.') + 1)}] {@m}\n{@x}";
        public const string HttpClientName = "Silk";
        public const string AutoModMessagePrefix = "[AUTOMOD]";
    }
}