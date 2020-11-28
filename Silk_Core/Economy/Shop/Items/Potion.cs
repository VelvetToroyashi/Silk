using DSharpPlus.CommandsNext;

namespace SilkBot.Economy.Shop.Items
{
    public class Potion : IBaseItem
    {
        public string Name { get; init; }
        public string Description { get; init; }
        public string ShortDescription { get; init; }

        public void Consume(CommandContext c) =>
            c.RespondAsync("You chug the potion with haste.").ConfigureAwait(false).GetAwaiter();
        
    }
}