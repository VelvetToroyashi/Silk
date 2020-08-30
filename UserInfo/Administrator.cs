using System;

namespace SilkBot.ServerConfigurations.UserInfo
{
    [Serializable]
    public class Administrator
    {
        public ulong ID { get; set; }
        public Administrator(ulong Id) => ID = Id;
    }
}
