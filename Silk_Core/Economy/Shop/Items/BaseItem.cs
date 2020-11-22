namespace SilkBot.Economy.Shop.Items
{
    public abstract class BaseItem
    {
        protected string Id             { get; init; }
        protected string Name           { get; init; }
        protected string Description    { get; init; }
    }
}