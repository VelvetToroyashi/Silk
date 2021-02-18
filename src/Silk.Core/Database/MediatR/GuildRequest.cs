using MediatR;
using Silk.Core.Database.Models;

namespace Silk.Core.Database.MediatR
{
    public class GuildRequest
    {
        public class AddGuildRequest : IRequest<Guild>
        {
            public ulong GuildId { get; init; }
        }
        
        
        public class GetOrCreateGuildRequest : IRequest<Guild>
        {
            public ulong GuildId { get; init; }        
        }
    }
}