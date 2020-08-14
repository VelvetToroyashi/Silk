using SilkBot.ServerConfigurations.UserInfo;
using SilkBot.UserInfo;
using System;
using System.Collections.Generic;

namespace SilkBot
{
    [Serializable]
    public class ServerConfig
    {
        public Administrator[] Administrators { get; set; }
        public BannedMember[] BannedMembers { get; set; }
        public List<ulong> SelfAssignableRoles { get; set; }
        public Moderator[] Moderators { get; set; }
        public ulong Guild { get; set; }
        public ulong LoggingChannel { get; set; }
        public ulong MutedRole { get; set; }
        public WhiteListUser PermittedUsers { get; set; }

    }
}




