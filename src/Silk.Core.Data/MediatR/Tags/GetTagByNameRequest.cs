using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Silk.Core.Data.Models;

namespace Silk.Core.Data.MediatR.Tags
{
    /// <summary>
    ///     Request for getting tags by name, or null, if none are found.
    /// </summary>
    public record GetTagByNameRequest(string Name, ulong GuildId) : IRequest<IEnumerable<Tag>?>;

    /// <summary>
    ///     The default handler for <see cref="GetTagByNameRequest" />.
    /// </summary>
    public class GetTagByNameHandler : IRequestHandler<GetTagByNameRequest, IEnumerable<Tag>?>
    {
        private readonly GuildContext _db;

        public GetTagByNameHandler(GuildContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<Tag>?> Handle(GetTagByNameRequest request, CancellationToken cancellationToken)
        {
            Tag[] tags = await _db
                .Tags
                .Include(t => t.Aliases)
                .Include(t => t.OriginalTag)
                .AsSplitQuery()
                .Where(t => EF.Functions.Like(t.Name.ToLower(), request.Name.ToLower() + '%'))
                .ToArrayAsync(cancellationToken);

            return tags.Any() ? tags : null;
        }
    }
}