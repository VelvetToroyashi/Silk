namespace Silk.Shared.Constants
{
	public static class StringConstants
	{

		public const string DefaultCommandPrefix = "s!";
		public const string Version = "2.2.16-hotfix";
		public const string LogFormat = "[{@t:h:mm:ss ff tt}] [{@l:u3}]{#if EventId is not null} [{EventId.Name}]{#end} [{Substring(SourceContext, LastIndexOf(SourceContext, '.') + 1)}] {@m}\n{@x}";
		public const string FileLogFormat = "[{Timestamp:h:mm:ss ff tt}] [{Level:u3}] [{SourceContext}] {Message:lj} {Exception:j}{NewLine}";
		public const string HttpClientName = "Silk";
		public const string AutoModMessagePrefix = "[AUTOMOD]";

		public const string ProjectIdentifier = "Silk! v" + Version + " By VelvetThePanda";
	}
}