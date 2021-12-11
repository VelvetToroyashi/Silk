using System;
using System.Threading.Tasks;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Rest.Core;
using Remora.Results;
using Silk.Core.Data.Entities;

namespace Silk.Core.Services.Interfaces;

/// <summary>
///     A service for applying infractions to userrs.
/// </summary>
public interface IInfractionService
{
	/// <summary>
	///     Automatically determines the appropriate infraction type and applies it to a user based on their pre-existing infractions, taking exemptions into
	///     account.
	/// </summary>
	/// <param name="guildID">The ID of the guild the infraction occurred on.</param>
	/// <param name="targetID">The ID of the target to infract.</param>
	/// <param name="enforcerID">The ID of the enforcer that invoked this.</param>
	/// <param name="reason">The reason the infraction is being given.</param>
	public Task<Result> AutoInfractAsync(Snowflake guildID, Snowflake targetID, Snowflake enforcerID, string reason = "Not Given.");

	/// <summary>
	///     Updates an existing infraction with a new reason or expiration.
	/// </summary>
	/// <param name="infraction">The infraction to update.</param>
	/// <param name="newReason">The new reason to apply to the infraction.</param>
	/// <param name="newExpiration">The new expiration to apply to the infraction.</param>
	/// <param name="updatedBy">The user that updated the infraction.</param>
	public Task<Result> UpdateInfractionAsync(InfractionEntity infraction, IUser updatedBy, string? newReason = null, Optional<TimeSpan?> newExpiration = default);

	/// <summary>
	///     Applies a strike to a user. Strikes may be used in automod actions to determine what action to take against a user.
	/// </summary>
	/// <param name="guildID">The ID of the guild the target is being striked on.</param>
	/// <param name="targetID">The ID of the target to be striked.</param>
	/// <param name="enforcerID">The ID of user striking the target.</param>
	/// <param name="reason">The reason strike was given.</param>
	public Task<Result> StrikeAsync(Snowflake guildID, Snowflake targetID, Snowflake enforcerID, string reason = "Not Given.");


	/// <summary>
	///     Kicks a member from the specified guild, generating an infraction.
	/// </summary>
	/// <param name="guildID">The ID of the guild the target will be kicked from.</param>
	/// <param name="targetID">The ID of the target to be kicked.</param>
	/// <param name="enforcerID">The ID of the user that kicked the target.</param>
	/// <param name="reason">The reason the target was kicked.</param>
	public Task<Result> KickAsync(Snowflake guildID, Snowflake targetID, Snowflake enforcerID, string reason = "Not Given.");

	/// <summary>
	///     Bans a member from the specified guild, generating an infraction.
	/// </summary>
	/// <param name="guildID">The ID of the guild the target is being banned from.</param>
	/// <param name="targetID">The ID of the target to ban.</param>
	/// <param name="enforcerID">The ID of the user that banned the target.</param>
	/// <param name="deleteDays">The number of days to delete messages from the target's messages.</param>
	/// <param name="reason">The reason the target was banned.</param>
	/// <param name="expirationRelativeToNow">A time relative to now to automatically unban the target.</param>
	public Task<Result> BanAsync(Snowflake guildID, Snowflake targetID, Snowflake enforcerID, int deleteDays = 0, string reason = "Not Given.", TimeSpan? expirationRelativeToNow = null);

	/// <summary>
	///     Unbans a user from the specified guild.
	/// </summary>
	/// <param name="guildID">The ID of the guild to unban the target from.</param>
	/// <param name="targetID">The ID of the target to unban.</param>
	/// <param name="enforcerID">The ID of user that unbanned the target.</param>
	/// <param name="reason">The reason the target was unbanned.</param>
	public Task<Result> UnBanAsync(Snowflake guildID, Snowflake targetID, Snowflake enforcerID, string reason = "Not Given.");



	/// <summary>
	///     Checks whether a the specified target has an active mute on the specified guild.
	/// </summary>
	/// <param name="guildID">The ID of the guild to check.</param>
	/// <param name="targetID">The ID of the target to check.</param>
	public ValueTask<bool> IsMutedAsync(Snowflake guildID, Snowflake targetID);

	/// <summary>
	///     Mutes a user on the specified guild.
	/// </summary>
	/// <param name="guildID">The ID of the guild the user should be muted on.</param>
	/// <param name="targetID">The ID of the target to be muted.</param>
	/// <param name="enforcerID">The ID of the user that invoked this action.</param>
	/// <param name="reason">The reason the target is being muted.</param>
	/// <param name="expirationRelativeToNow">Specifies a time relative to now for the user to be automatically unmuted at.</param>
	public Task<Result> MuteAsync(Snowflake guildID, Snowflake targetID, Snowflake enforcerID, string reason = "Not Given.", TimeSpan? expirationRelativeToNow = null);

	/// <summary>
	///     Attempts to unmute a user on the specified guild.
	/// </summary>
	/// <param name="guildID">The ID of the guild the mute was applied on.</param>
	/// <param name="targetID">The ID of the target to be unmuted.</param>
	/// <param name="enforcerID">The ID of user that invoked this action.</param>
	/// <param name="reason">The reason the target is being umuted.</param>
	public Task<Result> UnMuteAsync(Snowflake guildID, Snowflake targetID, Snowflake enforcerID, string reason = "Not Given.");

	/// <summary>
	///     Adds a note-infraction to the specified user. Notes are not taken into account when calculating automod actions.
	/// </summary>
	/// <param name="guildID">The ID of the guild this action was invoked from.</param>
	/// <param name="targetID">The ID of the target this action is taken against.</param>
	/// <param name="enforcerID">The ID of entity that invoked this action. The "moderator", in other words.</param>
	/// <param name="note">The content of the note to add to the target.</param>
	public Task<Result> AddNoteAsync(Snowflake guildID, Snowflake targetID, Snowflake enforcerID, string note);

	/// <summary>
	///     Pardons a user from a specific infraction. Only strikes that are held against the user can be pardoned.
	/// </summary>
	/// <param name="guildID">The ID of the guild the infraction occured on.</param>
	/// <param name="targetID">The ID of the target to be pardoned.</param>
	/// <param name="enforcerID">The ID of the entity pardoning the target.</param>
	/// <param name="caseID">The ID of the case to pardon the target from.</param>
	/// <param name="reason">The reason the pardon is being given.</param>
	public Task<Result> PardonAsync(Snowflake guildID, Snowflake targetID, Snowflake enforcerID, int caseID, string reason = "Not Given.");
}