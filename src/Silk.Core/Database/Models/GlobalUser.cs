using System;
using System.Collections.Generic;

namespace Silk.Core.Database.Models
{
    public class GlobalUser
    {
        public ulong Id { get; set; }
        public int Cash { get; set; }
        public DateTime LastCashOut { get; set; }

        public List<Item> Items { get; set; } = new();
    }
}