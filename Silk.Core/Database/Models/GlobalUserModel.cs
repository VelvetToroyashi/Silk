using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Silk.Core.Database.Models
{
    public class GlobalUserModel
    {
        [Key] 
        public ulong Id { get; set; }
        public int Cash { get; set; }
        public DateTime LastCashOut { get; set; }

        public List<ItemModel> Items { get; set; } = new();
    }
}