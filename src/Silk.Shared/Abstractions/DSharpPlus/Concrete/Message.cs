using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using Silk.Extensions.DSharpPlus;
using Silk.Shared.Abstractions.DSharpPlus.Interfaces;

namespace Silk.Shared.Abstractions.DSharpPlus.Concrete
{
    public class Message : IMessage
    {
        public ulong Id { get; }

        public ulong? GuildId { get; }

        public ulong ChannelId { get; }

        public IChannel Channel { get; }
        public IGuild? Guild { get; }

        public IUser Author { get; }

        public string Content => _message.Content;

        public DateTimeOffset CreationTimestamp { get; }

        //public IEmbed Embed { get; }

        public IMessage? Reply { get; }

        public IReadOnlyCollection<IEmoji> Reactions => ReactionUpdated() ? GetReactions() : _reactions;

        public IReadOnlyCollection<IUser> MentionedUsers => UsersUpdated() ? GetUsers() : _mentionedUsers;

        private bool _deleted;
        private readonly DiscordMessage _message;
        private IReadOnlyList<IEmoji> _reactions = new List<IEmoji>().AsReadOnly();
        private IReadOnlyList<IUser> _mentionedUsers = new List<IUser>().AsReadOnly();

        internal static Dictionary<ulong, Message> Messages { get; } = new();

        private Message(DiscordMessage message)
        {
            _message = message;
            Messages.Add(message.Id, this);

            Id = message.Id;
            GuildId = message.Channel.GuildId;
            ChannelId = message.ChannelId;

            Channel = (Channel) _message.Channel;
            Guild = (Guild?) _message.Channel.Guild;

            Author = (User) message.Author;
            CreationTimestamp = message.CreationTimestamp;
            Reply = (Message) message.ReferencedMessage!;
        }

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
                _deleted = true;
                await _message.DeleteAsync();
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

            await _message.ModifyAsync(m => m.Content = content);
        }

        public static implicit operator Message?(DiscordMessage? message)
        {
            if (message is null) return null;

            return Messages.TryGetValue(message.Id, out var msg) ? msg : new(message);
        }

        public static implicit operator DiscordMessage(Message message) =>
            (typeof(Message).GetField(nameof(_message), BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(message) as DiscordMessage)!;


        private bool ReactionUpdated() => _message.Reactions.Count != _reactions.Count;
        private IReadOnlyList<IEmoji> GetReactions() => _reactions = _message.Reactions.Select(r => (Emoji) r.Emoji).ToList().AsReadOnly();

        private bool UsersUpdated() => _message.MentionedUsers.Count != _mentionedUsers.Count;
        private IReadOnlyList<IUser> GetUsers() => _mentionedUsers = _message.MentionedUsers.Select(u => (User) u).ToList().AsReadOnly();
    }
}