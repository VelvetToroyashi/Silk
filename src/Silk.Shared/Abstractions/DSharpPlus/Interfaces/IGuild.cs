using System.Collections.Generic;

namespace Silk.Shared.Abstractions.DSharpPlus.Interfaces
{
    public interface IGuild
    {
        public ulong Id { get; }
        public IReadOnlyList<IUser> Users { get; }
        public IReadOnlyList<IChannel> Channels { get; }
        public IReadOnlyList<IEmoji> Emojis { get; }
        public IReadOnlyList<ulong> Roles { get; }
    }
}