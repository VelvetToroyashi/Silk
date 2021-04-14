using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using Silk.Shared.Abstractions.DSharpPlus.Interfaces;

namespace Silk.Shared.Abstractions.DSharpPlus.Concrete
{
    public class Message : IMessage
    {
        public Message(ulong id, ulong? guildId, IUser author, string? content, DateTimeOffset timestamp, IMessage? reply, IReadOnlyCollection<IEmoji> reactions)
        {
            Id = id;
            GuildId = guildId;
            Author = author;
            Content = content;
            Timestamp = timestamp;
            Reply = reply;
            Reactions = reactions;
        }
        public ulong Id { get; }

        public ulong? GuildId { get; }

        public IUser Author { get; }

        public string? Content { get; }

        public DateTimeOffset Timestamp { get; }

        public IMessage? Reply { get; }

        public IReadOnlyCollection<IEmoji> Reactions { get; }

        public async Task CreateReactionAsync(ulong emojiId) { }

        public static explicit operator Message?(DiscordMessage? message)
        {
            Message? reply = null;

            if (message is null) return null;
            if (message.ReferencedMessage is not null)
                reply = (Message) message.ReferencedMessage!;

            return new(
                message.Id,
                message.Channel.GuildId,
                (User) message.Author,
                message.Content,
                message.CreationTimestamp,
                reply, default!);
        }
    }
}