namespace SilkBot.Economy.Shop.Items
{
    public interface IBaseItem
    {
        public int Id { get; } 
        public string Name { get; init; }
        public string Description { get; init; }
        public string ShortDescription { get; init; }
        public static string BaseToString(IBaseItem i) => $"{i.Name} - {i.Description}";
    }
    
    
}