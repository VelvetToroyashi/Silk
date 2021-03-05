using System.Collections.Generic;
using MediatR;
using Silk.Data.Models;

namespace Silk.Data.MediatR
{
    public class GuildRequest
    {
        public record Get(ulong GuildId) : IRequest<Guild>;
        
        public record Add(ulong GuildId, string Prefix) : IRequest<Guild>;
        
        public record Update(ulong GuildId) : IRequest { public Infraction? Infraction { get; init; } }
        
        public record GetOrCreate(ulong GuildId, string Prefix) : IRequest<Guild>;

    }
}