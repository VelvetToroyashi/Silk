using System.Collections.Generic;

namespace Silk.Data.Models
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
        /// Whether to send a greeting message in the server when someone joins.
        /// </summary>
        public bool GreetMembers { get; set; }

        /// <summary>
        /// Whether to wait for the newly joined member to complete guild membership screening
        /// </summary>
        public bool GreetOnScreeningComplete { get; set; }

        /// <summary>
        /// Whether to greet when a specific role is granted to the user, rather than when they complete membership screening (if applicable).
        /// </summary>
        public bool GreetOnVerificationRole { get; set; }

        /// <summary>
        /// The Id of the role to wait for to be granted while
        /// </summary>
        public ulong VerificationRole { get; set; }

        /// <summary>
        /// Id of the channel to greet members in.
        /// </summary>
        public ulong GreetingChannel { get; set; }

        /// <summary>
        /// The text that will be used to greet new members.
        /// </summary>
        public string GreetingText { get; set; } = "";

        //This will be used eventually.
        //TODO: Have infraction type be it's own format
        public string InfractionFormat { get; set; } = "";

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
        public ulong LoggingChannel { get; set; }
        /// <summary>
        /// Whether to log when messages are edited/deleted.
        /// </summary>
        public bool LogMessageChanges { get; set; }
        /// <summary>
        /// Whether to log members joining or not.
        /// </summary>
        public bool LogMemberJoing { get; set; }

        /// <summary>
        /// Blacklist certain invites.
        /// </summary>
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
        // ReSharper disable once CollectionNeverUpdated.Global
        public List<BlacklistedWord> BlackListedWords { get; set; } = new();

        /// <summary>
        /// A list of disabled commands on this server
        /// </summary>
        public List<DisabledCommand> DisabledCommands { get; set; } = new();
        /// <summary>
        /// A list of roles that can be obtained from Silk!.
        /// </summary>
        public List<SelfAssignableRole> SelfAssignableRoles { get; set; } = new();
    }
}