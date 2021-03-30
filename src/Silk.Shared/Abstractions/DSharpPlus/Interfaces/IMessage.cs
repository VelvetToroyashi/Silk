using System.Collections.Generic;

namespace Silk.Shared.Abstractions.DSharpPlus.Interfaces
{
    public interface IMessage
    {
        public ulong Id { get; internal set; }
        public ulong UserId { get; internal set; }
        public IReadOnlyCollection<IEmoji> Reactions { get; set; }
    }
}