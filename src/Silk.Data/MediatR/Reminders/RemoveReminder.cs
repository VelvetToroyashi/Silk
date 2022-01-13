using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Silk.Data.Entities;

namespace Silk.Data.MediatR.Reminders;

public static class RemoveReminder
{
    /// <summary>
    /// Request to remove a reminder.
    /// </summary>
    public sealed record Request(int ReminderId) : IRequest;

    /// <summary>
    /// The default handler for <see cref="Request" />.
    /// </summary>
    internal sealed class Handler : IRequestHandler<Request>
    {
        private readonly GuildContext _db;

        public Handler(GuildContext db) => _db = db;

        public async Task<Unit> Handle(Request request, CancellationToken cancellationToken)
        {
            ReminderEntity? reminder = await _db.Reminders
                                                .FirstOrDefaultAsync(r => r.Id == request.ReminderId, cancellationToken);

            if (reminder is not null)
            {
                _db.Reminders.Remove(reminder);
                try
                {
                    await _db.SaveChangesAsync(cancellationToken);
                }
                // Timer timed out and it got dequeued slower than it should've. //
                catch (DbUpdateConcurrencyException) { }
            }

            return new();
        }
    }
}