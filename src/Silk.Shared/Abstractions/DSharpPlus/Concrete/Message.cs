using System;
using System.Collections.Generic;
using DSharpPlus.Entities;
using Silk.Shared.Abstractions.DSharpPlus.Interfaces;

namespace Silk.Shared.Abstractions.DSharpPlus.Concrete
{
    public class Message : IMessage
    {
        public Message(ulong id, IUser author, string? content, DateTimeOffset timestamp, IMessage? reply, IReadOnlyCollection<IEmoji> reactions)
        {
            ((IMessage) this).Id = id;
            ((IMessage) this).Author = author;
            ((IMessage) this).Content = content;
            ((IMessage) this).Timestamp = timestamp;
            ((IMessage) this).Reply = reply;
            ((IMessage) this).Reactions = reactions;
        }

        ulong IMessage.Id { get; set; }

        IUser IMessage.Author { get; set; }

        string IMessage.Content { get; set; }

        DateTimeOffset IMessage.Timestamp { get; set; }

        IMessage? IMessage.Reply { get; set; }

        IReadOnlyCollection<IEmoji> IMessage.Reactions { get; set; }

        public static explicit operator Message?(DiscordMessage? message)
        {
            return message is null ?
                null :
                new(message.Id, (User) message.Author, message.Content, message.CreationTimestamp, message.ReferencedMessage is null ? null : (Message?) message.ReferencedMessage, default!);
        }
    }
}