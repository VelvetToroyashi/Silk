using DSharpPlus.CommandsNext;

namespace SilkBot.Economy.Shop.Items.Interfaces
{
    public interface IBaseItem
    {
        public string Name { get; init; }
        public string Description { get; init; }
        public string ShortDescription { get; init; }
        public static string BaseToString(IBaseItem i) => $"{i.Name} - {i.Description}";

        public void Consume(CommandContext c);
    }
    
    
}