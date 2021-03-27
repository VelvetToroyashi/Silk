using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Silk.Core.Data.Models;

namespace Silk.Core.Data.MediatR.Unified.Reminders
{
    /// <summary>
    /// Gets all reminders.
    /// </summary>
    public record ReminderGetAllRequest : IRequest<IEnumerable<Reminder>>;

    /// <summary>
    /// The default handler for <see cref="ReminderGetAllRequest"/>.
    /// </summary>
    public class ReminderGetAllHandler : IRequestHandler<ReminderGetAllRequest, IEnumerable<Reminder>>
    {
        private readonly GuildContext _db;

        public ReminderGetAllHandler(GuildContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<Reminder>> Handle(ReminderGetAllRequest request,
            CancellationToken cancellationToken)
        {
            return await _db.Reminders
                .Include(r => r.Owner)
                .ToListAsync(cancellationToken);
        }
    }
}