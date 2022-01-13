using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Remora.Rest.Core;
using Silk.Data.Entities;

namespace Silk.Data.MediatR.Tags;

public static class CreateTag
{
    /// <summary>
    /// Request for creating a <see cref="TagEntity" />.
    /// </summary>
    public sealed record Request(string Name, Snowflake GuildID, Snowflake OwnerID, string Content, TagEntity? OriginalTag) : IRequest<TagEntity>;

    /// <summary>
    /// The default handler for <see cref="Request" />
    /// </summary>
    internal sealed class Handler : IRequestHandler<Request, TagEntity>
    {
        private readonly GuildContext _db;

        public Handler(GuildContext db) => _db = db;

        public async Task<TagEntity> Handle(Request request, CancellationToken cancellationToken)
        {
            TagEntity tag = new()
            {
                OwnerID       = request.OwnerID,
                GuildID       = request.GuildID,
                Name          = request.Name,
                OriginalTagId = request.OriginalTag?.Id,
                Content       = request.Content,
                CreatedAt     = DateTime.UtcNow,
                Aliases       = request.OriginalTag is null ? new() : null
            };

            _db.Tags.Add(tag);
            await _db.SaveChangesAsync(cancellationToken);

            return tag;
        }
    }
}