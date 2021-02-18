using MediatR;
using Silk.Core.Database.Models;

namespace Silk.Core.Database.MediatR
{
    public class AddUserRequest : IRequest<User>
    {
        public ulong GuildId { get; init; }
        public ulong UserId { get; init; }
        public UserFlag? Flags { get; init; }
    }
}