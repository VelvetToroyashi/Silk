using System.Collections.Generic;
using MediatR;
using Silk.Data.Models;

namespace Silk.Data.MediatR
{
    public class UserRequest
    {
        public class Get : IRequest<User?>
        {
            public ulong UserId { get; init; }
            public ulong GuildId { get; init; }
        }
        
        public class Update : IRequest<User>
        {
            public ulong UserId { get; init; }
            public ulong GuildId { get; init; }
            public UserFlag? Flags { get; init; }
            public List<Infraction>? Infractions { get; init; }
        }
        
        public class Add : IRequest<User>
        {
            public ulong GuildId { get; init; }
            public ulong UserId { get; init; }
            public UserFlag? Flags { get; init; }
        }
    }
}