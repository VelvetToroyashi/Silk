using System;
using System.Collections.Generic;
using System.Text;

namespace SilkBot.ServerConfigurations.UserInfo
{
    [Serializable]
    public class BannedMember
    {
        public ulong ID { get; set; }
        public string Reason { get; set; }
        
    }
}
