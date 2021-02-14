using System;

namespace Silk.Core.Database.Models
{
    public class GlobalUser
    {
        public ulong Id { get; set; }
        public int Cash { get; set; }
        public DateTime LastCashOut { get; set; }
    }
}