namespace Silk.Shared.Abstractions.DSharpPlus
{
    public interface IEmoji
    {
        public ulong Id { get; init; }
        public string Name { get; init; }
    }
}