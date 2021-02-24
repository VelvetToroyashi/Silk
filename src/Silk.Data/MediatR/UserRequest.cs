using System.Collections.Generic;
using MediatR;
using Silk.Data.Models;

namespace Silk.Data.MediatR
{
    public class UserRequest
    {
        public record Get(ulong GuildId, ulong UserId) : IRequest<User?>;
        
        public record Update(ulong GuildId, ulong UserId) : IRequest<User>
        {
            public UserFlag? Flags { get; init; }
            public List<Infraction>? Infractions { get; init; }
        }

        public record Add(ulong GuildId, ulong UserId, UserFlag? Flags) : IRequest<User>;
        
        public record GetOrCreate(ulong GuildId, ulong UserId) : IRequest<User>
        {
            public UserFlag? Flags { get; init; }
            public List<Infraction>? Infractions { get; init; }
        }
        
    }
}