using System;
using System.Collections.Generic;
using System.Text;

namespace SilkBot.Commands.Economy.Shop
{
    public class Shop
    {
        //The last time the hourly shop is refreshed//
        public DateTime lastHourlyRefresh { get; set; }
        //The last time the daily shop is refreshed//
        public  DateTime LastFullRefresh { get; set; }
        //Maximum amount of items that are allowed to appear on the shop at a given time.//
        private const int MAX_ITEMS = 8;
        //Previously circulated items.//
        private List<IShopObject> circulatedItems;
        //Currently circulated items.//
        private List<IShopObject> itemsInCirculation;

        private List<IShopObject> refreshableItems;


        public IEnumerable<IShopObject> GetCurrentItems()
        {
            return itemsInCirculation;
        }

        public void CheckShopStatus()
        {
            if (RefreshHourly(DateTime.Now))
            {
                var rand = new Random();
                var numOfItems = rand.Next(4, 7);
                FlushItems(numOfItems);
                PopulateShop(numOfItems, rand);
                lastHourlyRefresh = DateTime.Now;
            }
        }


        private bool RefreshHourly(DateTime currentTime)
        {
            var delta = currentTime - TimeSpan.FromHours(1);
            return delta > lastHourlyRefresh;
        }


        private void PopulateShop(int numberOfItems, Random rand)
        {
            if (numberOfItems + circulatedItems.Count > MAX_ITEMS) 
                throw new OverflowException("Items exceeded shop limit.");
            
            for (var i = 0; i < numberOfItems; i++)
            {
                var randomNum = rand.Next(0, refreshableItems.Count);
                if (circulatedItems[i] != refreshableItems[randomNum])
                    circulatedItems.Add(refreshableItems[randomNum]);
            }
        }
        private void FlushItems(int numberOfItems)
        {
            circulatedItems.RemoveRange(0, numberOfItems);
        }




    }
}
