namespace Silk.Shared.Abstractions.DSharpPlus
{
    public interface IReaction
    {
        public IEmoji Emoji { get; internal set; }
        public ulong UserId { get; set; }
    }
}