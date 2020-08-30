using DSharpPlus.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace SilkBot.Commands.Economy.Shop
{
    public abstract class BaseShop
    {
        //The last time the hourly shop is refreshed//
        public DateTime LastHourlyRefresh { get; set; }
        //The last time the daily shop is refreshed//
        public  DateTime LastFullRefresh { get; set; }

        public DiscordEmbed ShopUI { get; private set; }

        //Maximum amount of items that are allowed to appear on the shop at a given time.//
        protected readonly int MAX_ITEMS;
        //Previously circulated items.//
        protected readonly List<IShopItem> circulatedItems = new List<IShopItem>();
        //Currently circulated items.//
        protected readonly List<IShopItem> itemsInCirculation = new List<IShopItem>();

        protected readonly List<IShopItem> refreshableItems = new List<IShopItem>();





        public BaseShop(int maxItems)
        {
            MAX_ITEMS = maxItems;
        }




        private protected IEnumerable<IShopItem> LoadShopFromConfigurationFile(string path) =>
            JsonConvert.DeserializeObject<IEnumerable<IShopItem>>(path);
        

        public IEnumerable<IShopItem> GetCurrentItems()
        {
            return itemsInCirculation;
        }

        public virtual void CheckShopStatus()
        {
            if (RefreshHourlyRefreshedItems(DateTime.Now))
            {
                var rand = new Random();
                var numOfItems = rand.Next(4, 7);
                CirculateItems(numOfItems);
                PopulateEmptyShopSlots(numOfItems, rand);
                LastHourlyRefresh = DateTime.Now;
            }
        }


        protected bool RefreshHourlyRefreshedItems(DateTime currentTime)
        {
            var delta = currentTime - TimeSpan.FromHours(1);
            return delta > LastHourlyRefresh;
        }


        protected void PopulateEmptyShopSlots(int numberOfItems, Random rand)
        {
            if (numberOfItems + itemsInCirculation.Count > MAX_ITEMS) 
                throw new OverflowException("Items exceeded shop limit.");
            
            for (var i = 0; i < numberOfItems; i++)
            {
                var randomNum = rand.Next(0, refreshableItems.Count);
                if(itemsInCirculation.Count < 1)
                {
                    itemsInCirculation.Add(refreshableItems[randomNum]);
                    continue;
                }
                if (itemsInCirculation[i] != refreshableItems[randomNum])
                    itemsInCirculation.Add(refreshableItems[randomNum]);
            }
        }
        private void CirculateItems(int numberOfItems)
        {
            circulatedItems.AddRange(itemsInCirculation.GetRange(0, numberOfItems));
            itemsInCirculation.RemoveRange(0, numberOfItems);
        }




    }
}
