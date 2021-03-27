using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Silk.Core.Data.Models;

namespace Silk.Core.Data.MediatR.Unified.Tags
{
    public record TagUpdateRequest(string Name, ulong GuildId) : IRequest<Tag>
    {
        public string? NewName { get; init; }
        public int? Uses { get; init; }
        public string? Content { get; init; }
        public List<Tag>? Aliases { get; init; }
    }

    public class TagUpdateHandler : IRequestHandler<TagUpdateRequest, Tag>
    {
        private readonly GuildContext _db;

        public TagUpdateHandler(GuildContext db)
        {
            _db = db;
        }

        public async Task<Tag> Handle(TagUpdateRequest request, CancellationToken cancellationToken)
        {
            Tag tag = await _db.Tags.FirstAsync(t => t.Name.ToLower() == request.Name.ToLower() &&
                                                     t.GuildId == request.GuildId, cancellationToken);
            tag.Name = request.NewName ?? tag.Name;
            tag.Uses = request.Uses ?? tag.Uses;
            tag.Content = request.Content ?? tag.Content;
            tag.Aliases = request.Aliases ?? tag.Aliases;

            await _db.SaveChangesAsync(cancellationToken);
            return tag;
        }
    }
}