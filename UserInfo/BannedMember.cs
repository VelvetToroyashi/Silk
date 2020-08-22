using System;

namespace SilkBot.ServerConfigurations.UserInfo
{
    [Serializable]
    public class BannedMember
    {
        public ulong ID { get; set; }
        public string Reason { get; set; }
        public DateTime? ExpirationDate { get; private set; }

        public BannedMember(ulong Id, string reason = "Not given", DateTime? expiration = null)
        {
            ID = Id;
            Reason = reason;
            ExpirationDate = expiration;
        }
    }
}
