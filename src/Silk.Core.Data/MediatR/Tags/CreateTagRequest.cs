using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Silk.Core.Data.Entities;

namespace Silk.Core.Data.MediatR.Tags;

/// <summary>
///     Request for creating a <see cref="TagEntity" />.
/// </summary>
public record CreateTagRequest(string Name, ulong GuildId, ulong OwnerId, string Content, TagEntity? OriginalTag) : IRequest<TagEntity>;

/// <summary>
///     The default handler for <see cref="CreateTagRequest" />
/// </summary>
public class CreateTagHandler : IRequestHandler<CreateTagRequest, TagEntity>
{
    private readonly GuildContext _db;

    public CreateTagHandler(GuildContext db) => _db = db;

    public async Task<TagEntity> Handle(CreateTagRequest request, CancellationToken cancellationToken)
    {
        TagEntity tag = new()
        {
            OwnerId       = request.OwnerId,
            GuildId       = request.GuildId,
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