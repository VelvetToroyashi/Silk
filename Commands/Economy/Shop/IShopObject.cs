namespace SilkBot.Commands.Economy.Shop
{
    public interface IShopItem
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int Price { get; set; }
    }
}