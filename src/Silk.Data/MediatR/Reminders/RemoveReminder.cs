using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Remora.Results;
using Silk.Data.Entities;

namespace Silk.Data.MediatR.Reminders;

public static class RemoveReminder
{
    /// <summary>
    /// Request to remove a reminder.
    /// </summary>
    public sealed record Request(int ReminderId) : IRequest<Result>;

    /// <summary>
    /// The default handler for <see cref="Request" />.
    /// </summary>
    internal sealed class Handler : IRequestHandler<Request, Result>
    {
        private readonly IDbContextFactory<GuildContext> _dbFactory;

        public async Task<Result> Handle(Request request, CancellationToken cancellationToken)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            
            ReminderEntity? reminder = await db.Reminders.FirstOrDefaultAsync(r => r.Id == request.ReminderId, cancellationToken);

            if (reminder is null)
            {
                return Result.FromError(new NotFoundError($"A reminder with the ID of {request.ReminderId} does not exist."));
            }
            
            db.Reminders.Remove(reminder);
            
            try
            {
                await db.SaveChangesAsync(cancellationToken);
            }
            // Timer timed out and it got dequeued slower than it should've. //
            catch (DbUpdateConcurrencyException) { }

            return Result.FromSuccess();
        }
    }
}