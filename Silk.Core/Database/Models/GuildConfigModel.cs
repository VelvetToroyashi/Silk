using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Silk.Core.Database.Models
{
    public class GuildConfigModel
    {
        
        [Key] 
        public ulong Id { get; set; }
        public ulong MuteRoleId { get; set; } 
        
        public ulong GreetingChannel { get; set; }

        #region AutoMod/Moderation

        public ulong GeneralLoggingChannel { get; set; }
        public bool LogMessageChanges { get; set; }
        public bool GreetMembers { get; set; }
        public bool LogRoleChange { get; set; }

        public bool BlacklistInvites { get; set; }
        public bool BlacklistWords { get; set; }
        
        public bool UseAggressiveRegex { get; set; }

        #endregion

        

        #region Premium Features
        
        public bool IsPremium { get; set; }
        
        public bool AutoDehoist { get; set; }

        public bool ScanInvites { get; set; }
        
        public List<GuildInviteModel> AllowedInvites { get; set; }
        

        #endregion
        
        public string InfractionFormat { get; set; } = string.Empty;


        
        public List<BlackListedWord> BlackListedWords { get; set; } = new();
        public List<WhiteListedLink> WhiteListedLinks { get; set; } = new();
        public List<SelfAssignableRole> SelfAssignableRoles { get; set; } = new();
        public List<Ban> Bans { get; set; } = new();
        
        public GuildModel Guild { get; set; }
        // [ForeignKey("Guild")]
        // public GuildModel Guild { get; set; }
    }
}