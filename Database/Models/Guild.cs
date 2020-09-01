using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SilkBot.Models
{
    public class Guild
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public ulong DiscordGuildId { get; set; }

        public bool LogMessageChanges { get; set; }
        public bool LogMemberJoinOrLeave { get; set; }
        [Required]
        [StringLength(5)]
        public string Prefix { get; set; }

        public ulong? MuteRoleID { get; set; }
        public ulong? MessageEditChannel { get; set; }
        public ulong? GeneralLoggingChannel { get; set; }
        public ulong? MemberLeaveJoinChannel { get; set; }

        public List<SelfAssignableRole> SelfAssignableRoles { get; set; } = new List<SelfAssignableRole>();
        public List<Ban> Bans { get; set; } = new List<Ban>();
        public List<DiscordUserInfo> DiscordUserInfos { get; set; } = new List<DiscordUserInfo>();
        
    }
}
