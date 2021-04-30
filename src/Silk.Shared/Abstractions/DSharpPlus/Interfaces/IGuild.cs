using System.Collections.Generic;

namespace Silk.Shared.Abstractions.DSharpPlus.Interfaces
{
    public interface IGuild
    {
        public ulong Id { get; }
        public IDictionary<ulong, IUser> Users { get; }
        public IDictionary<ulong, IChannel> Channels { get; }
        public IReadOnlyList<IEmoji> Emojis { get; }
        public IReadOnlyList<ulong> Roles { get; }
    }
}