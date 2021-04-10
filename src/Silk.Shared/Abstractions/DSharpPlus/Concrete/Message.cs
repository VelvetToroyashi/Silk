using System;
using System.Collections.Generic;
using DSharpPlus.Entities;
using Silk.Shared.Abstractions.DSharpPlus.Interfaces;

namespace Silk.Shared.Abstractions.DSharpPlus.Concrete
{
    public class Message : IMessage
    {
        private IUser _author;
        private string _content;
        private ulong _id;
        private IReadOnlyCollection<IEmoji> _reactions;
        private IMessage? _reply;
        private DateTimeOffset _timestamp;

        public Message(ulong id, IUser author, string? content, DateTimeOffset timestamp, IMessage? reply, IReadOnlyCollection<IEmoji> reactions)
        {
            _id = id;
            _author = author;
            _content = content;
            _timestamp = timestamp;
            _reply = reply;
            _reactions = reactions;
        }

        ulong IMessage.Id
        {
            get => _id;
            set => _id = value;
        }

        IUser IMessage.Author
        {
            get => _author;
            set => _author = value;
        }

        string IMessage.Content
        {
            get => _content;
            set => _content = value;
        }

        DateTimeOffset IMessage.Timestamp
        {
            get => _timestamp;
            set => _timestamp = value;
        }

        IMessage? IMessage.Reply
        {
            get => _reply;
            set => _reply = value;
        }

        IReadOnlyCollection<IEmoji> IMessage.Reactions
        {
            get => _reactions;
            set => _reactions = value;
        }

        public static explicit operator Message?(DiscordMessage? message)
        {
            return message is null ?
                null :
                new(message.Id, (User) message.Author, message.Content, message.CreationTimestamp, message.ReferencedMessage is null ? null : (Message) message.ReferencedMessage, default!);
        }
    }
}