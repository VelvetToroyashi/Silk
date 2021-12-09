using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Remora.Rest.Core;
using Silk.Core.Data.Entities;

namespace Silk.Core.Data.MediatR.Tags;

/// <summary>
///     Request to update a <see cref="TagEntity" />.
/// </summary>
/// <param name="Name">The Name of the Tag</param>
/// <param name="GuildId">The Id of the Guild</param>
public record UpdateTagRequest(string Name, ulong GuildId) : IRequest<TagEntity>
{
    public Optional<ulong>           OwnerId { get; init; }
    public Optional<string>          NewName { get; init; }
    public Optional<int>             Uses    { get; init; }
    public Optional<string>          Content { get; init; }
    public Optional<List<TagEntity>> Aliases { get; init; }
}

/// <summary>
///     The default handler for <see cref="T:Silk.Core.Data.MediatR.Tags.UpdateTagRequest" />.
/// </summary>
public sealed class UpdateTagHandler : IRequestHandler<UpdateTagRequest, TagEntity>
{
    private readonly GuildContext _db;
    public UpdateTagHandler(GuildContext db) => _db = db;

    public async Task<TagEntity> Handle(UpdateTagRequest request, CancellationToken cancellationToken)
    {
        TagEntity tag = await _db.Tags
            .Include(t => t.Aliases)
            .FirstAsync(t => t.Name == request.Name && t.GuildId == request.GuildId, cancellationToken);

        if (request.Uses.IsDefined(out var uses))
            tag.Uses = uses;
            
        if (request.Content.IsDefined(out var content))
            tag.Content = content;
            
        if (request.OwnerId.IsDefined(out var ownerId))
            tag.OwnerId = ownerId;
            
        if (request.NewName.IsDefined(out var newName))
        {
            tag.Name = newName;
            tag.Aliases?.ForEach(a => a.Name = newName);
        }
            
        if (request.Aliases.IsDefined(out var aliases))
            tag.Aliases = aliases;
            
        if (request.Aliases.IsDefined(out aliases))
            foreach (var alias in aliases)
                alias.Content = tag.Content;
            
        await _db.SaveChangesAsync(cancellationToken);

        return tag;
    }
}