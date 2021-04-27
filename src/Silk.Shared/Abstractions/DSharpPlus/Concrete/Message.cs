using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using Silk.Extensions.DSharpPlus;
using Silk.Shared.Abstractions.DSharpPlus.Interfaces;

namespace Silk.Shared.Abstractions.DSharpPlus.Concrete
{
    public class Message : IMessage
    {
        private bool _deleted;
        private readonly DiscordMessage _message;

        public Message(DiscordMessage message)
        {
            _message = message;

            Id = message.Id;
            GuildId = message.Channel.GuildId;
            ChannelId = message.ChannelId;
            Author = (User) message.Author;
            Content = message.Content;
            Timestamp = message.CreationTimestamp;
            Reply = (Message) message.ReferencedMessage!;
            Reactions = default!;
        }

        public ulong Id { get; }

        public ulong? GuildId { get; }
        /// <inheritdoc />
        public ulong ChannelId { get; }

        public IUser Author { get; }

        public string? Content { get; private set; }

        public DateTimeOffset Timestamp { get; }

        //public IEmbed Embed { get; }

        public IMessage? Reply { get; }

        public IReadOnlyCollection<IEmoji> Reactions { get; }

        public async Task CreateReactionAsync(ulong emojiId)
        {
            if (!_deleted)
            {
                var client = _message.GetClient();
                var emoji = DiscordEmoji.FromGuildEmote(client, emojiId);
                await _message.CreateReactionAsync(emoji);
            }
        }

        public async Task CreateReactionAsync(IEmoji emoji)
        {
            if (!_deleted)
            {
                if (emoji is not Emoji e)
                    throw new InvalidCastException($"Canont convert from {emoji.GetType().Name} to {nameof(Emoji)}");

                await _message.CreateReactionAsync(e);
            }
        }

        public Task RemoveReactionsAsync() => _message.DeleteAllReactionsAsync();

        public async Task DeleteAsync()
        {
            try
            {
                await _message.DeleteAsync();
                _deleted = true;
            }
            catch (NotFoundException)
            {
                /* Ignored. */
            }
        }

        public async Task EditAsync(string content)
        {
            if (_deleted)
                throw new InvalidOperationException("Cannot modify content of deleted message.");

            Content = content;
            await _message.ModifyAsync(m => m.Content = content);
        }

        public static implicit operator Message?(DiscordMessage? message)
        {
            if (message is null) return null;

            return new(message);
        }
    }
}