using SilkBot.ServerConfigurations.UserInfo;
using System;
using System.Collections.Generic;

namespace SilkBot
{
    [Serializable]
    public class ServerConfig
    {
        public ulong Guild { get; set; }
        public BannedMember[] BannedMembers { get; set; }
        public Moderator[] Moderators { get; set; }
        public Administrator[] Administrators { get; set; }
        public ulong MutedRole { get; set; }
        public List<ulong> SelfAssignableRoles { get; set; }
        public ulong LoggingChannel { get; set; }
    }
}




