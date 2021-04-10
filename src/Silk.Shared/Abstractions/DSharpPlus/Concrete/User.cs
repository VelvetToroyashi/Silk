using DSharpPlus.Entities;
using Silk.Shared.Abstractions.DSharpPlus.Interfaces;

namespace Silk.Shared.Abstractions.DSharpPlus.Concrete
{
    public class User : IUser
    {
        public ulong Id { get; init; }

        public static explicit operator User(DiscordUser u)
        {
            return new() {Id = u.Id};
        }
    }
}