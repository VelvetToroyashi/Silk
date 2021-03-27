using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Silk.Core.Data.Models;

namespace Silk.Core.Data.MediatR.Unified.Reminders
{
    /// <summary>
    /// Removes a reminder.
    /// </summary>
    public record ReminderRemoveRequest(int ReminderId) : IRequest;

    /// <summary>
    /// The default handler for <see cref="ReminderRemoveRequest"/>.
    /// </summary>
    public class ReminderRemoveHandler : IRequestHandler<ReminderRemoveRequest>
    {
        private readonly GuildContext _db;

        public ReminderRemoveHandler(GuildContext db)
        {
            _db = db;
        }

        public async Task<Unit> Handle(ReminderRemoveRequest request, CancellationToken cancellationToken)
        {
            Reminder? reminder = await _db.Reminders.FirstOrDefaultAsync(r => r.Id == request.ReminderId, cancellationToken);
            if (reminder is not null)
            {
                _db.Reminders.Remove(reminder);
                try
                {
                    await _db.SaveChangesAsync(cancellationToken);
                }
                // Timer timed out and it got dequeued slower than it should've. //
                catch (DbUpdateConcurrencyException)
                {
                }
            }

            return new();
        }
    }
}