using MediatR;
using Silk.Data.Models;

namespace Silk.Data.MediatR
{
    public class GuildRequest
    {
        public class AddGuildRequest : IRequest<Guild>
        {
            public ulong GuildId { get; init; }
            public string Prefix { get; init; } = null!;
        }
        
        
        public class GetOrCreateGuildRequest : IRequest<Guild>
        {
            public ulong GuildId { get; init; }
            public string Prefix { get; init; } = null!;
            
        }
    }
}