namespace Silk.Core.Types
{
	/// <summary>
	/// An enum containing semantic values of the result of trying to apply a moderation action to a user.
	/// </summary>
	public enum InfractionResult
	{
		/// <summary>
		/// The operation failed because the necessary configuration was not set up.
		/// </summary>
		FailedNotConfigured,
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
		FailedGuildMemberCache,
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
		/// <summary>
		/// The operation <i>likely</i> succeeded, but could not log to the configured log channel.
		/// </summary>
		FailedLogPermissions,
		/// <summary>
		/// The operation failed due to a requirement not being met, but does not warrant a specific return value.
		/// </summary>
		FailedGenericRequirementsNotFulfilled
	}
}