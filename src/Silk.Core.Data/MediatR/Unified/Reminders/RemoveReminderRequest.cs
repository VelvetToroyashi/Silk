using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Silk.Core.Data.Models;

namespace Silk.Core.Data.MediatR.Unified.Reminders
{
    /// <summary>
    /// Request to remove a reminder.
    /// </summary>
    public record RemoveReminderRequest(int ReminderId) : IRequest;

    /// <summary>
    /// The default handler for <see cref="RemoveReminderRequest"/>.
    /// </summary>
    public class RemoveReminderHandler : IRequestHandler<RemoveReminderRequest>
    {
        private readonly GuildContext _db;

        public RemoveReminderHandler(GuildContext db)
        {
            _db = db;
        }

        public async Task<Unit> Handle(RemoveReminderRequest request, CancellationToken cancellationToken)
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