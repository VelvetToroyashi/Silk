using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Silk.Core.Database.Models
{
    public class GuildConfigModel
    {
        // Database requisites. //
        [Key] public int ConfigId { get; set; }
        public ulong GuildId { get; set; }
        public GuildModel Guild { get; set; }
        public ulong MuteRoleId { get; set; }
        public ulong GreetingChannel { get; set; }
        public string InfractionFormat { get; set; }
        #region AutoMod/Moderation

        public int MaxMentionsInMessage { get; set; }
        public ulong GeneralLoggingChannel { get; set; }
        public bool LogMessageChanges { get; set; }
        public bool GreetMembers { get; set; }
        public bool LogRoleChange { get; set; }
        public bool BlacklistInvites { get; set; }
        public bool BlacklistWords { get; set; }
        public bool WarnOnMatchedInvite { get; set; }
        public bool DeleteMessageOnMatchedInvite { get; set; }
        public bool UseAggressiveRegex { get; set; }

        #endregion
        
        #region Premium Features

        public bool IsPremium { get; set; }
        public bool AutoDehoist { get; set; }
        public bool ScanInvites { get; set; }

        #endregion
        public List<GuildInviteModel> AllowedInvites { get; set; } = new();
        public List<BlackListedWord> BlackListedWords { get; set; } = new();
        public List<WhiteListedLink> WhiteListedLinks { get; set; } = new();
        public List<SelfAssignableRole> SelfAssignableRoles { get; set; } = new();
    }
}