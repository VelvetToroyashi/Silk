using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Silk.Core.Database.Models
{
    public class GuildConfigModel
    {
        #region Ids

        public ulong MuteRoleId { get; set; } 
        public ulong GeneralLoggingChannel { get; set; }
        public ulong GreetingChannel { get; set; }

        public bool LogMessageChanges { get; set; }

        public bool GreetMembers { get; set; }
        public bool LogRoleChange { get; set; }
        
        #endregion

        #region Toggles
        
        public bool WhitelistInvites { get; set; }
        public bool BlacklistWords { get; set; }
        public bool AutoDehoist { get; set; }
        public bool IsPremium { get; set; }

        #endregion
        [Key] public ulong Id { get; set; }
        
        

        
        [Required] 
        [StringLength(5)] 
        public string Prefix { get; set; }

        public string InfractionFormat { get; set; } = string.Empty;


        
        public List<BlackListedWord> BlackListedWords { get; set; } = new();
        public List<WhiteListedLink> WhiteListedLinks { get; set; } = new();
        public List<SelfAssignableRole> SelfAssignableRoles { get; set; } = new();
        public List<Ban> Bans { get; set; } = new();
        
        
        public GuildModel Guild { get; set; }
    }
}