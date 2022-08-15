using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Silk.Data.Entities;

namespace Silk.Data.MediatR.Reminders;

public static class GetAllReminders
{
    /// <summary>
    /// Request for getting all reminders.
    /// </summary>
    public sealed record Request(int ShardCount, int ShardID) : IRequest<IEnumerable<ReminderEntity>>;

    /// <summary>
    /// The default handler for <see cref="Request" />.
    /// </summary>
    internal sealed class Handler : IRequestHandler<Request, IEnumerable<ReminderEntity>>
    {
        private readonly IDbContextFactory<GuildContext> _dbFactory;
        public Handler(IDbContextFactory<GuildContext> dbFactory) => _dbFactory = dbFactory;

        public async Task<IEnumerable<ReminderEntity>> Handle(Request request, CancellationToken cancellationToken)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            
            return await db.Reminders
                           .FromSqlRaw("SELECT * FROM reminders r WHERE (COALESCE(r.guild_id, 0)::bigint >> 22) % {0} = {1}", request.ShardCount, request.ShardID)
                           .ToListAsync(cancellationToken);
        }
    }
}