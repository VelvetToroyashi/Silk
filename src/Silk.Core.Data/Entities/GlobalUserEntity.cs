using System;

namespace Silk.Core.Data.Entities
{
    public class GlobalUserEntity
    {
        public ulong    Id          { get; set; }
        public int      Cash        { get; set; }
        public DateTime LastCashOut { get; set; }
    }
}