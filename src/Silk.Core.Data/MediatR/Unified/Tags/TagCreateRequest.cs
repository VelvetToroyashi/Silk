using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Silk.Core.Data.Models;

namespace Silk.Core.Data.MediatR.Unified.Tags
{
    /// <summary>
    /// Creates a <see cref="Tag"/>.
    /// </summary>
    public record TagCreateRequest(string Name, ulong GuildId, ulong OwnerId, string Content, Tag? OriginalTag) : IRequest<Tag>;

    /// <summary>
    /// The default handler for <see cref="TagCreateRequest"/>
    /// </summary>
    public class TagCreateHandler : IRequestHandler<TagCreateRequest, Tag>
    {
        private readonly GuildContext _db;

        public TagCreateHandler(GuildContext db)
        {
            _db = db;
        }

        public async Task<Tag> Handle(TagCreateRequest request, CancellationToken cancellationToken)
        {
            Tag tag = new()
            {
                OwnerId = request.OwnerId,
                GuildId = request.GuildId,
                Name = request.Name,
                OriginalTagId = request.OriginalTag?.Id,
                Content = request.Content,
                CreatedAt = DateTime.UtcNow,
                Aliases = request.OriginalTag is null ? new() : null
            };

            _db.Tags.Add(tag);
            await _db.SaveChangesAsync(cancellationToken);

            return tag;
        }
    }
}