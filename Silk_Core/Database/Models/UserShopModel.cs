using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using SilkBot.Economy.Shop;

namespace SilkBot.Database.Models
{
    public class UserShopModel 
    {
        [Key]
        public ulong            OwnerId     { get; init; }
        public DateTime         Created     { get; init; }
        public int              ItemsSold   { get; set;  }
        public bool             IsPremium   { get; set;  }
        public bool             IsPrivate   { get; set;  }
        public List<ShopItem>   Items       { get; set;  } = new();
    }
}