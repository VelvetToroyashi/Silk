using MediatR;
using Silk.Data.Models;

namespace Silk.Data.MediatR
{
    public class TagRequest
    {
        public record Get(string Name, ulong GuildId) : IRequest<Tag?>;
        public record Update(string Name, ulong GuildId, int Uses, string Content) : IRequest<Tag>;
        public record Create(string Name, ulong GuildId, ulong OwnerId, string Content) : IRequest<Tag>; 
        public record Delete(string Name, ulong GuildId) : IRequest;
    }
}