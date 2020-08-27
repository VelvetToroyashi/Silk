using Newtonsoft.Json;
using SilkBot.ServerConfigurations.UserInfo;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SilkBot
{
    [Serializable]
    public class GuildInfo
    {
        [JsonProperty(PropertyName = "Admins")]
        public List<Administrator> Administrators { get; set; }
        [JsonProperty(PropertyName = "Banned Members")]
        public List<BannedMember> BannedMembers { get; set; }
        [JsonProperty(PropertyName = "Self assignable role Ids")]
        public List<ulong> SelfAssignableRoles { get; set; }
        [JsonProperty(PropertyName = "Moderators")]
        public List<Moderator> Moderators { get; set; }
        [JsonProperty(PropertyName = "Guild Id")]
        public ulong Guild { get; set; }
        [JsonProperty(PropertyName = "Log channel")]
        public ulong LoggingChannel { get; set; }
        [JsonProperty(PropertyName = "Mute role Id")]
        public ulong MutedRole { get; set; }

        public ulong MemberChangedNotificationChannel { get; set; }
        public bool NotifyOnGuildMemberCountChanges { get; set; }

        public IEnumerable<ulong> GetStaffMembers()
        {
            var adminIds = Administrators.Select(admin => admin.ID);
            var modIds = Moderators.Select(mod => mod.ID);
            return modIds.Union(adminIds);
        }


    }
}