using System;
using SilkBot.Economy.Shop.Items;

namespace SilkBot.Commands.Economy.Shop
{
    public class ShopItem 
    {
        private readonly IBaseItem _item;

        public string Name { get => _item.Name; }
        public string Description { get => _item.ShortDescription; }

        public int Amount { get; set; }
        

        public ShopItem(IBaseItem item, int initialAmount)
        {
            _item = item;
            Amount = initialAmount;
        }

    }
}