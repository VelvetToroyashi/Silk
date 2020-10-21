using System;

namespace SilkBot.UserInfo
{
    [Serializable]
    public class Administrator
    {
        public ulong ID { get; set; }
        public Administrator(ulong Id) => ID = Id;
    }
}
