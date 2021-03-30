namespace Silk.Shared.Abstractions.DSharpPlus.Interfaces
{
    public interface IReaction
    {
        public IEmoji Emoji { get; internal set; }
        public ulong UserId { get; set; }
    }
}