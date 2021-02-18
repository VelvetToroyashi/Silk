using System.Collections.Generic;
using MediatR;
using Silk.Data.Models;

namespace Silk.Data.MediatR
{
    public class UserRequest
    {
        public class GetUserRequest : IRequest<User>
        {
            public ulong UserId { get; init; }
            public ulong GuildId { get; init; }
        }
        
        public class UpdateUserRequest : IRequest<User>
        {
            public ulong UserId { get; init; }
            public UserFlag? Flags { get; init; }
            public List<Infraction>? Infractions { get; init; }
        }
        
        public class AddUserRequest : IRequest<User>
        {
            public ulong GuildId { get; init; }
            public ulong UserId { get; init; }
            public UserFlag? Flags { get; init; }
        }
    }
}