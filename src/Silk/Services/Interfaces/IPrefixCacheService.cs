using System.Threading.Tasks;
using Remora.Rest.Core;
using Silk.Data.Entities;

namespace Silk.Services.Interfaces;

/// <summary>
///     Provides methods for updating and retrieving a
///     <see cref="GuildEntity" />'s <seealso cref="string" /> prefix, which is used for command recognition.
/// </summary>
public interface IPrefixCacheService
{
	/// <summary>
	///     Retrieve the corresponding prefix for a <see cref="GuildEntity" />.
	/// </summary>
	/// <param name="guildId">The Id of the guild to retrieve the prefix for.</param>
	/// <returns>The prefix configured for the guild.</returns>
	public ValueTask<string> RetrievePrefixAsync(Snowflake? guildId);

	/// <summary>
	///     Update a prefix for a <see cref="GuildEntity" />.
	/// </summary>
	/// <param name="guildId">The Id of the guild to update.</param>
	/// <param name="prefix">The prefix to assign to the guild.</param>
	public void UpdatePrefix(Snowflake guildId, string prefix);
}