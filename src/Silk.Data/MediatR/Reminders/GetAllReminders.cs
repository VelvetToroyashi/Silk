using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Mediator;
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
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal sealed class Handler : IRequestHandler<Request, IEnumerable<ReminderEntity>>
    {
        private readonly GuildContext _db;
        public Handler(GuildContext db) => _db = db;

        public async ValueTask<IEnumerable<ReminderEntity>> Handle(Request request, CancellationToken cancellationToken)
        {
            
            
            return await _db.Reminders
                           .FromSqlRaw("SELECT * FROM reminders r WHERE (COALESCE(r.guild_id, 0)::bigint >> 22) % {0} = {1}", request.ShardCount, request.ShardID)
                           .ToListAsync(cancellationToken);
        }
    }
}