using System.Collections.Generic;
using MediatR;
using Silk.Data.Models;

namespace Silk.Data.MediatR
{
    public class TagRequest
    {
        public record  Get(string Name, ulong GuildId) : IRequest<Tag?>;
        public record GetByUser(ulong GuildId, ulong OwnerId) : IRequest<IEnumerable<Tag>?>;
        public record GetByName(string Name, ulong GuildId) : IRequest<IEnumerable<Tag>?>;
        
        public record Update(string Name, ulong GuildId) : IRequest<Tag>
        {
            public string? NewName { get; init; }
            public int? Uses { get; init; }
            public string? Content { get; init; }
            public List<Tag>? Aliases { get; init; }
        }
        public record Create(string Name, ulong GuildId, ulong OwnerId, string Content, Tag? OriginalTag) : IRequest<Tag>; 
        public record Delete(string Name, ulong GuildId) : IRequest;
    }
}