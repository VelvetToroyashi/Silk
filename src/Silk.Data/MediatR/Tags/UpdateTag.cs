using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Remora.Rest.Core;
using Silk.Data.Entities;

namespace Silk.Data.MediatR.Tags;

public static class UpdateTag
{
    /// <summary>
    /// Request to update a <see cref="TagEntity" />.
    /// </summary>
    /// <param name="Name">The Name of the Tag</param>
    /// <param name="GuildID">The Id of the Guild</param>
    public sealed record Request(string Name, Snowflake GuildID) : IRequest<TagEntity>
    {
        public Optional<Snowflake>       OwnerID { get; init; }
        public Optional<string>          NewName { get; init; }
        public Optional<int>             Uses    { get; init; }
        public Optional<string>          Content { get; init; }
        public Optional<List<TagEntity>> Aliases { get; init; }
    }
    
    /// <summary>
    /// The default handler for <see cref="T:Silk.Data.MediatR.Tags.Request" />.
    /// </summary>
    internal sealed class Handler : IRequestHandler<Request, TagEntity>
    {
        private readonly GuildContext _db;
        public Handler(GuildContext db) => _db = db;
    
        public async Task<TagEntity> Handle(Request request, CancellationToken cancellationToken)
        {
            TagEntity tag = await _db.Tags
                                     .Include(t => t.Aliases)
                                     .FirstAsync(t => t.Name == request.Name && t.GuildID == request.GuildID, cancellationToken);
    
            if (request.Uses.IsDefined(out int uses))
                tag.Uses = uses;
    
            if (request.Content.IsDefined(out string? content))
                tag.Content = content;
    
            if (request.OwnerID.IsDefined(out Snowflake ownerId))
                tag.OwnerID = ownerId;
    
            if (request.NewName.IsDefined(out string? newName))
            {
                tag.Name = newName;
                tag.Aliases?.ForEach(a => a.Name = newName);
            }
    
            if (request.Aliases.IsDefined(out List<TagEntity>? aliases))
                tag.Aliases = aliases;
    
            if (request.Aliases.IsDefined(out aliases))
                foreach (TagEntity alias in aliases)
                    alias.Content = tag.Content;
    
            await _db.SaveChangesAsync(cancellationToken);
    
            return tag;
        }
    }
}