#region

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

#endregion

namespace Silk.Core.Database.Models
{
    [DisplayColumn("Discord Guilds")]
    public class GuildModel
    {
        [Key] public ulong Id { get; set; }
        [Display(Name = "Whitelist Invites")] public bool WhitelistInvites { get; set; }
        [Display(Name = "Blacklist Enabled")] public bool BlacklistWords { get; set; }
        [Display(Name = "Auto-Dehoist")] public bool AutoDehoist { get; set; }

        [Display(Name = "Log Message Edit/Deletion")]
        public bool LogMessageChanges { get; set; }

        public bool GreetMembers { get; set; }
        public bool LogRoleChange { get; set; }
        [Required] [StringLength(5)] public string Prefix { get; set; }

        public string InfractionFormat { get; set; } = string.Empty;

        public ulong MuteRoleId { get; set; } 
        public ulong MessageEditChannel { get; set; }

        public ulong GeneralLoggingChannel { get; set; }
        public ulong GreetingChannel { get; set; }
        public List<BlackListedWord> BlackListedWords { get; set; } = new();
        public List<WhiteListedLink> WhiteListedLinks { get; set; } = new();
        public List<SelfAssignableRole> SelfAssignableRoles { get; set; } = new();
        public List<Ban> Bans { get; set; } = new();
        public List<UserModel> Users { get; set; } = new();
    }
}