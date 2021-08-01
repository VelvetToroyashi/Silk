using System.Collections.Generic;

namespace Silk.Core.Data.Models
{
	public class GuildModConfig
	{
		public int Id { get; set; }
		public ulong GuildId { get; set; }
		
		#region AutoMod/Moderation
		
		/// <summary>
		/// Id of the role to apply when muting members.
		/// </summary>
		public ulong MuteRoleId { get; set; }
		
        /// <summary>
        /// A list of whitelisted invites.
        /// </summary>
        public List<Invite> AllowedInvites { get; set; } = new();

        /// <summary>
        /// The maximum amount of users that can be mentioned in a single message.
        /// </summary>
        public int MaxUserMentions { get; set; }

        /// <summary>
        /// The maximum amount of roles that can be mentioned in a single role.
        /// </summary>
        public int MaxRoleMentions { get; set; }

        /// <summary>
        ///     Channel Id to log moderation/message changes to.
        /// </summary>
        public ulong LoggingChannel { get; set; }

        /// <summary>
        ///     Whether to log when messages are edited/deleted.
        /// </summary>
        public bool LogMessageChanges { get; set; }

        /// <summary>
        ///     Whether to log members joining or not.
        /// </summary>
        public bool LogMemberJoins { get; set; }

        /// <summary>
        ///     Whether to log members leaving or not.
        /// </summary>
        public bool LogMemberLeaves { get; set; }

        /// <summary>
        ///     Blacklist certain invites.
        /// </summary>
        public bool BlacklistInvites { get; set; }

        /// <summary>
        ///     Blacklist certain words.
        /// </summary>
        public bool BlacklistWords { get; set; }

        /// <summary>
        ///     Represents whether to add an infraction to the user after sending an invite.
        /// </summary>
        public bool WarnOnMatchedInvite { get; set; }

        /// <summary>
        ///     Represents whether to delete a message containing an invite.
        /// </summary>
        public bool DeleteMessageOnMatchedInvite { get; set; }

        /// <summary>
        ///     Represents whether to match only discord.gg/ or all possible invite codes.
        /// </summary>
        public bool UseAggressiveRegex { get; set; }

        /// <summary>
        /// Whether or not infractions will escalate based on the number of infractions a user has.
        /// </summary>
        public bool AutoEscalateInfractions { get; set; }

        /// <summary>
        ///     Whether to automatically de-hoist members. Guild must be premium.
        /// </summary>
        public bool AutoDehoist { get; set; }

        /// <summary>
        ///     Whether to scan matched invites. Server must be premium and blacklist invites.
        /// </summary>
        public bool ScanInvites { get; set; }

        /// <summary>
        ///     A list of steps depending on the number of infractions a <see cref="User" /> has.
        /// </summary>
        public List<InfractionStep> InfractionSteps { get; set; } = new();

        #endregion
	}
}