using System.Collections.Generic;
using Silk.Shared.Abstractions.DSharpPlus.Interfaces;

namespace Silk.Shared.Abstractions.DSharpPlus.Concrete
{
    public class Guild : IGuild
    {
        public ulong Id { get; init; }
        public IReadOnlyList<IUser> Users { get; init; }
        public IReadOnlyList<IChannel> Channels { get; init; }
    }
}