using System;

namespace SilkBot.Models
{
    public class UserInfractionModel
    {
        public int Id { get; set; }
        public string Reason { get; set; }
        public ulong Enforcer { get; set; }
        public DiscordUserInfo User { get; set; }
        public DateTime InfractionTime { get; set; }
    }
}
