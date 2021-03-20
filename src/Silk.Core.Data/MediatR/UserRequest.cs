using MediatR;
using Silk.Core.Data.Models;

namespace Silk.Core.Data.MediatR
{
    public class UserRequest
    {
        public record Get(ulong GuildId, ulong UserId) : IRequest<User>;

        public record Update(ulong GuildId, ulong UserId, UserFlag? Flags) : IRequest<User>;

        public record Add(ulong GuildId, ulong UserId, UserFlag? Flags) : IRequest<User>;
        
        public record GetOrCreate(ulong GuildId, ulong UserId) : IRequest<User>
        {
            public UserFlag? Flags { get; init; }
        }
        
    }
}