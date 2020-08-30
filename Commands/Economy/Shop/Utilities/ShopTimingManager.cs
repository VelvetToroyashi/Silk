using System;
using System.Collections.Generic;
using System.Timers;

namespace SilkBot.Commands.Economy.Shop.Utilities
{
    public sealed class ShopTimingManager
    {
        private readonly Timer timer = new Timer(3600000);
        public List<BaseShop> Shops { get; set; }

        public ShopTimingManager() => timer.Elapsed += (s, e) => Shops.ForEach(shop =>
        {
            shop.CheckShopStatus();
            SilkBot.Bot.Instance.Client.DebugLogger.LogMessage(DSharpPlus.LogLevel.Info, "Silk!", "Updated a shop!", DateTime.Now);
        });
    }
}