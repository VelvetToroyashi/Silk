namespace Silk.Core.Types
{
	/// <summary>
	/// An enum containing semantic values of the result of trying to apply a moderation action to a user.
	/// </summary>
	public enum InfractionResult
	{
		/// <summary>
		/// Heirarchy [of roles] prevented an operation from being performed.
		/// </summary>
		FailedGuildHeirarchy,
		/// <summary>
		/// The current member does not have requisite permissions to perform the action.
		/// </summary>
		FailedSelfPermissions,
		/// <summary>
		/// The member the infraction applied to left before or during the operation.
		/// </summary>
		FailedMemberGuildCache,
		/// <summary>
		/// A necessary resource was configured, but did not exist.
		/// </summary>
		FailedResourceDeleted,
		/// <summary>
		/// The operation succeeded, but does not notify the member the infraction applies to.
		/// </summary>
		SucceededDoesNotNotify,
		/// <summary>
		/// The operation succeeded, and the member the infraction was applied to was notified.
		/// </summary>
		SucceededWithNotification,
		/// <summary>
		/// The operation succeeded, but the member the infraction applies to was not notified.
		/// </summary>
		SucceededWithoutNotification,
	}
}