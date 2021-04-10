using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Silk.Core.Data.Models;

namespace Silk.Core.Data.MediatR.Unified.Tags
{
    /// <summary>
    ///     Request to update a <see cref="Tag" />.
    /// </summary>
    /// <param name="Name">The Name of the Tag</param>
    /// <param name="GuildId">The Id of the Guild</param>
    public record UpdateTagRequest(string Name, ulong GuildId) : IRequest<Tag>
    {
        public string? NewName { get; init; }
        public int? Uses { get; init; }
        public string? Content { get; init; }
        public List<Tag>? Aliases { get; init; }
    }

    /// <summary>
    ///     The default handler for <see cref="UpdateTagRequest" />.
    /// </summary>
    public class UpdateTagHandler : IRequestHandler<UpdateTagRequest, Tag>
    {
        private readonly GuildContext _db;

        public UpdateTagHandler(GuildContext db)
        {
            _db = db;
        }

        public async Task<Tag> Handle(UpdateTagRequest request, CancellationToken cancellationToken)
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