using System.Collections.Generic;

namespace Silk.Core.Database.Models
{
    public class GuildConfig
    {
        // Database requisites. //
        /// <summary>
        /// The Primary Key (PK) of the model.
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Requisite property to form a Foreign Key (FK)
        /// </summary>
        public ulong GuildId { get; set; }
        /// <summary>
        /// Requisite property to form a Foreign Key (FK)
        /// </summary>
        public Guild Guild { get; set; }
        /// <summary>
        /// Id of the role to apply when muting members.
        /// </summary>
        public ulong MuteRoleId { get; set; }
        /// <summary>
        /// Id of the channel to greet members in.
        /// </summary>
        public ulong GreetingChannel { get; set; }

        //This will be used eventually.
        public string InfractionFormat { get; set; } = string.Empty;

        #region AutoMod/Moderation

        /// <summary>
        /// The maximum amount of users that can be mentioned in a single message. 
        /// </summary>
        public int MaxUserMentions { get; set; }
        /// <summary>
        /// The maximum amount of roles that can be mentioned in a single role.
        /// </summary>
        public int MaxRoleMentions { get; set; }
        /// <summary>
        /// Channel Id to log moderation/message changes to.
        /// </summary>
        public ulong GeneralLoggingChannel { get; set; }
        /// <summary>
        /// Whether to log when messages are edited/deleted.
        /// </summary>
        public bool LogMessageChanges { get; set; }
        /// <summary>
        /// Whether to send a greeting message in the server when someone joins.
        /// </summary>
        public bool GreetMembers { get; set; }
        public bool BlacklistInvites { get; set; }
        /// <summary>
        /// Blacklist certain words.
        /// </summary>
        public bool BlacklistWords { get; set; }
        /// <summary>
        /// Represents whether to add an infraction to the user after sending an invite.
        /// </summary>
        public bool WarnOnMatchedInvite { get; set; }
        /// <summary>
        /// Represents whether to delete a message containing an invite.
        /// </summary>
        public bool DeleteMessageOnMatchedInvite { get; set; }
        /// <summary>
        /// Represents whether to match only discord.gg/ or all possible invite codes.
        /// </summary>
        public bool UseAggressiveRegex { get; set; }

        /// <summary>
        /// A list that contians the type of infraction that should be applied, and an expiration if applicable.
        /// </summary>
        public List<InfractionType> InfractionDictionary { get; set; } = new();

        #endregion

        #region Premium Features

        /// <summary>
        /// Whether a guild is considered premium.
        /// </summary>
        public bool IsPremium { get; set; }
        /// <summary>
        /// Whether to automatically dehoist members. Guild must be premium.
        /// </summary>
        public bool AutoDehoist { get; set; }
        /// <summary>
        /// Whether to scan matched invites. Server must be premium and blacklist invites.
        /// </summary>
        public bool ScanInvites { get; set; }

        #endregion
        /// <summary>
        /// A list of whitlisted invites.
        /// </summary>
        public List<Invite> AllowedInvites { get; set; } = new();
        /// <summary>
        /// A list of blacklisted words.
        /// </summary>
        public List<BlackListedWord> BlackListedWords { get; set; } = new();
        /// <summary>
        /// A list of roles that can be obtained from Silk!.
        /// </summary>
        public List<SelfAssignableRole> SelfAssignableRoles { get; set; } = new();
    }
}