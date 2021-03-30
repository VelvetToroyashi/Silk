using System;
using System.Collections.Generic;

namespace Silk.Shared.Abstractions.DSharpPlus.Interfaces
{
    public interface IMessage
    {
        public ulong Id { get; internal set; }
        public IUser Author { get; internal set; }
        public string Content { get; internal set; }
        public DateTimeOffset Timestamp { get; internal set; }
        public IMessage? Reply { get; internal set; }
        public IReadOnlyCollection<IEmoji> Reactions { get; internal set; }
    }
}