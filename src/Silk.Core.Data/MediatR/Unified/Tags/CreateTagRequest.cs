using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Silk.Core.Data.Models;

namespace Silk.Core.Data.MediatR.Unified.Tags
{
    /// <summary>
    ///     Request for creating a <see cref="Tag" />.
    /// </summary>
    public record CreateTagRequest(string Name, ulong GuildId, ulong OwnerId, string Content, Tag? OriginalTag) : IRequest<Tag>;

    /// <summary>
    ///     The default handler for <see cref="CreateTagRequest" />
    /// </summary>
    public class CreateTagHandler : IRequestHandler<CreateTagRequest, Tag>
    {
        private readonly GuildContext _db;

        public CreateTagHandler(GuildContext db)
        {
            _db = db;
        }

        public async Task<Tag> Handle(CreateTagRequest request, CancellationToken cancellationToken)
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