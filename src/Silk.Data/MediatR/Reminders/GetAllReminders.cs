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
    public sealed record Request : IRequest<IEnumerable<ReminderEntity>>;

    /// <summary>
    /// The default handler for <see cref="Request" />.
    /// </summary>
    internal sealed class Handler : IRequestHandler<Request, IEnumerable<ReminderEntity>>
    {
        private readonly GuildContext _db;
        public Handler(GuildContext db) => _db = db;

        public async Task<IEnumerable<ReminderEntity>> Handle(Request request, CancellationToken cancellationToken) 
            => await _db.Reminders.ToListAsync(cancellationToken);
    }
}