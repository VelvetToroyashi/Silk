using System.ComponentModel.DataAnnotations.Schema;
using SilkBot.Economy.Shop.Items.Interfaces;

namespace SilkBot.Economy.Shop
{
    public class ShopItem 
    {
        public int Id { get; set; }
        [NotMapped]
        public IBaseItem Item { get; set; }
        public int ItemId { get; set; }
        
        public string Name { get; }
        public string Description { get; }
        public int Price { get; set; }
        
    }
}