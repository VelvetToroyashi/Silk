using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Remora.Rest.Core;
using Silk.Data.Entities;

namespace Silk.Data.MediatR.Tags;

public static class GetTag
{
    /// <summary>
    /// Request to get a <see cref="TagEntity" />, or null if it doesn't exist.
    /// </summary>
    public sealed record Request(string Name, Snowflake GuildId) : IRequest<TagEntity?>;

    /// <summary>
    /// The default handler for <see cref="Request" />.
    /// </summary>
    internal sealed class Handler : IRequestHandler<Request, TagEntity?>
    {
        private readonly GuildContext _db;

        public Handler(GuildContext db) => _db = db;

        public async Task<TagEntity?> Handle(Request request, CancellationToken cancellationToken)
        {
            TagEntity? tag = await _db.Tags
                                      .Include(t => t.OriginalTag)
                                      .Include(t => t.Aliases)
                                      .AsSplitQuery()
                                      .FirstOrDefaultAsync(t =>
                                                               EF.Functions.Like(t.Name.ToLower(), request.Name.ToLower() + '%') &&
                                                               t.GuildID == request.GuildId, cancellationToken);

            return tag;
        }
    }
}