using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Silk.Core.Data.Models;

namespace Silk.Core.Data.MediatR.Tags
{
    /// <summary>
    ///     Request to delete a <see cref="Tag" />.
    /// </summary>
    public record DeleteTagRequest(string Name, ulong GuildId) : IRequest;

    /// <summary>
    ///     The default handler for <see cref="DeleteTagRequest" />.
    /// </summary>
    public class DeleteTagHandler : IRequestHandler<DeleteTagRequest>
    {
        private readonly GuildContext _db;

        public DeleteTagHandler(GuildContext db)
        {
            _db = db;
        }

        public async Task<Unit> Handle(DeleteTagRequest request, CancellationToken cancellationToken)
        {
            Tag tag = await _db
                .Tags
                .Include(t => t.OriginalTag)
                .Include(t => t.Aliases)
                .AsSplitQuery()
                .FirstOrDefaultAsync(t =>
                    t.Name.ToLower() == request.Name.ToLower() &&
                    t.GuildId == request.GuildId, cancellationToken);

            Tag[] aliasedTags = await _db
                .Tags
                .Include(t => t.OriginalTag)
                .Include(t => t.Aliases)
                .AsSplitQuery()
                .Where(t => t.OriginalTagId == tag.Id || t.OriginalTag!.OriginalTagId == tag.Id)
                .ToArrayAsync(cancellationToken);


            foreach (Tag t in aliasedTags)
                _db.Tags.Remove(t);

            _db.Tags.Remove(tag);
            await _db.SaveChangesAsync(cancellationToken);
            return new();
        }
    }
}