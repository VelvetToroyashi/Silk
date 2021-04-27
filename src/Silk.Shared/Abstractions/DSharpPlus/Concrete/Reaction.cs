using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Silk.Shared.Abstractions.DSharpPlus.Interfaces;

namespace Silk.Shared.Abstractions.DSharpPlus.Concrete
{
    public class Reaction : IReaction
    {
        public IEmoji Emoji { get; }

        public ulong UserId { get; }

        private readonly DiscordEmoji _emoji;
        private readonly DiscordMessage _message;

        private Reaction(MessageReactionAddEventArgs reaction)
        {
            _message = reaction.Message;
            _emoji = reaction.Emoji;

            Emoji = (Emoji) reaction.Emoji;
            UserId = reaction.User.Id;
        }

        public Task DeleteAsync() => _message.DeleteReactionsEmojiAsync(_emoji);

        public static implicit operator Reaction(MessageReactionAddEventArgs reaction) => new(reaction);
    }
}