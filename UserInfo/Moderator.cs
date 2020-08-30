using System;
using System.Collections.Generic;
using System.Text;

namespace SilkBot.ServerConfigurations.UserInfo
{
    [Serializable]
    public class Moderator
    {
        public ulong ID { get; set; }
        public Moderator(ulong ID) => this.ID = ID;
    }
}
