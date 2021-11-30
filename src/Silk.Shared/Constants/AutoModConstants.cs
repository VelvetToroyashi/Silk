using System.Collections.Concurrent;

namespace Silk.Shared.Constants
{
	/// <summary>
	///     <para>A class that defines all string keys for AutoMod actions.</para>
	///     <para>If a key is not present in a config dictionary, the config's configured InfractionStep is used. </para>
	/// </summary>
	public static class AutoModConstants
	{
		/// <summary>
		///     The maximum amount of unique user mentions a message can contain.
		/// </summary>
		public const string MaxUserMentions = "max_user_mentions";

		/// <summary>
		///     The maximum amount of unique role mentions a message can contain.
		/// </summary>
		public const string MaxRoleMentions = "max_role_mentions";

		public const string PhishingLinkDetected = "phishing_link";

		//TODO: More auto-mod strings

		public static ConcurrentDictionary<string, string> ActionStrings { get; } = new()
		{
			[MaxUserMentions] = "Maximum unique user pings",
			[MaxRoleMentions] = "Maximum unique role pings",
		};
	}
}