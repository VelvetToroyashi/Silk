using System.Collections.Generic;

namespace Silk.Core.Data.Entities
{
    public class GuildModConfigEntity
    {
        public int         Id      { get; set; }
        public ulong       GuildId { get; set; }
        public GuildEntity Guild   { get; set; }

        /// <summary>
        ///     Id of the role to apply when muting members.
        /// </summary>
        public ulong MuteRoleId { get; set; }

        /// <summary>
        ///     A list of whitelisted invites.
        /// </summary>
        public List<InviteEntity> AllowedInvites { get; set; } = new();

        /// <summary>
        ///     The maximum amount of users that can be mentioned in a single message.
        /// </summary>
        public int MaxUserMentions { get; set; }

        /// <summary>
        ///     The maximum amount of roles that can be mentioned in a single role.
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
        ///     Whether or not infractions will escalate based on the number of infractions a user has.
        /// </summary>
        public bool AutoEscalateInfractions { get; set; }

        /// <summary>
        ///     Whether to automatically de-hoist members.
        /// </summary>
        public bool AutoDehoist { get; set; }

        /// <summary>
        ///     All active auto-mod exemptions on the guild.
        /// </summary>
        public List<ExemptionEntity> Exemptions { get; set; } = new();

        /// <summary>
        ///     Whether or not to even scan for phishing links on a server.
        /// </summary>
        public bool DetectPhishingLinks { get; set; }

        /// <summary>
        ///     Whether or not phishing links should be deleted.
        /// </summary>
        public bool DeletePhishingLinks { get; set; }

        /// <summary>
        ///     Whether to scan matched invites. Server must be premium and blacklist invites.
        /// </summary>
        public bool ScanInvites { get; set; }

        /// <summary>
        ///     Whether to use webhooks to log infractions.
        /// </summary>
        public bool UseWebhookLogging { get; set; }

        /// <summary>
        ///     The id of the webhook to log with.
        /// </summary>
        public ulong WebhookLoggingId { get; set; }

        /// <summary>
        ///     The url of the webhook to log with, if applicable.
        /// </summary>
        public string? LoggingWebhookUrl { get; set; }

        /// <summary>
        ///     Gets various logging-related settings.
        /// </summary>
        public GuildLoggingConfigEntity LoggingConfigEntity { get; set; }

        /// <summary>
        ///     A list of steps depending on the number of infractions a <see cref="UserEntity" /> has.
        /// </summary>
        public List<InfractionStepEntity> InfractionSteps { get; set; } = new();

        /// <summary>
        ///     A dictionary containing "named" infraction steps, where the key to the dictionary represents a discriminated action,
        ///     which may have different consequences than another. In the event a key is not found in this dictionary, it should be assumed the action would
        ///     result to <see cref="InfractionType.Strike" />.
        /// </summary>
        public Dictionary<string, InfractionStepEntity> NamedInfractionSteps { get; set; } = new();
    }
}