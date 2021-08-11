
using System.Collections.Concurrent;

namespace Silk.Shared.Constants
{
	/// <summary>
	/// <para>A class that defines all string keys for AutoMod actions.</para>
	/// 
	/// <para>If a key is not present in a config dictionary, the config's configured InfractionStep is used. </para>
	/// </summary>
	public static class AutoModConstants
	{
		/// <summary>
		/// The maximum amount of unique user mentions a message can contain.
		/// </summary>
		public const string MaxUserMentions = "max_user_mentions";
		
		/// <summary>
		/// The maximum amount of unique role mentions a message can contain.
		/// </summary>
		public const string MaxRoleMentions = "max_role_mentions";
		
		//TODO: More automod strings

		public static ConcurrentDictionary<string, string> ActionStrings { get; } = new()
		{
			[MaxUserMentions] = "Maxiumim unique user pings",
			[MaxRoleMentions] = "Maxiumim unique role pings",
		};
	}
}