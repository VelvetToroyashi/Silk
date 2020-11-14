using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SilkBot.Models
{
    [DisplayColumn("Discord Guilds")]
    public class GuildModel
    {
        [Key]
        public ulong Id { get; set; }
        [Display(Name = "Whitelist Invites")]
        public bool WhitelistInvites { get; set; }
        [Display(Name = "Blacklist Enabled")]
        public bool BlacklistWords { get; set; }
        [Display(Name = "Auto-Dehoist")]
        public bool AutoDehoist { get; set; }
        [Display(Name = "Log Message Edit/Deletion")]
        public bool LogMessageChanges { get; set; }
        public bool GreetMembers { get; set; }
        public bool LogRoleChange { get; set; }
        [Required]
        [StringLength(5)]
        public string Prefix { get; set; }

        public string InfractionFormat { get; set; }

        public ulong MuteRoleId { get; set; }
        public ulong MessageEditChannel { get; set; }

        public ulong GeneralLoggingChannel { get; set; }
        public ulong GreetingChannel { get; set; }
        public List<BlackListedWord> BlackListedWords { get; set; } = new List<BlackListedWord>();
        public List<WhiteListedLink> WhiteListedLinks { get; set; } = new List<WhiteListedLink>();
        public List<SelfAssignableRole> SelfAssignableRoles { get; set; } = new List<SelfAssignableRole>();
        public List<Ban> Bans { get; set; } = new List<Ban>();
        public List<UserModel> Users { get; set; } = new List<UserModel>();

    }
}
