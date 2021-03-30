using System.Collections.Generic;

namespace Silk.Shared.Abstractions.DSharpPlus
{
    public interface IGuild
    {
        public ulong Id { get; init; }
        public IReadOnlyList<IUser> Users { get; init; }
        public IReadOnlyList<IChannel> Channels { get; init; }
    }
}