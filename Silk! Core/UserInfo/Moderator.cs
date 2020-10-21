using System;

namespace SilkBot.UserInfo
{
    [Serializable]
    public class Moderator
    {
        public ulong ID { get; set; }
        public Moderator(ulong ID) => this.ID = ID;
    }
}
