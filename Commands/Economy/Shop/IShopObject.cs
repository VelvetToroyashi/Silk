using System;
using System.Collections.Generic;
using System.Text;

namespace SilkBot.Commands.Economy.Shop
{

    public interface IShopObject
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int Price { get; set; }
    }
}
