using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Silk.Data.Entities;

namespace Silk.Data.MediatR.Reminders;

public static class UpdateReminder
{
    public sealed record Request(ReminderEntity Reminder, DateTime Expiration) : IRequest<ReminderEntity>;

    internal sealed class Handler : IRequestHandler<Request, ReminderEntity>
    {
        private readonly GuildContext _db;
        public Handler(GuildContext db) => _db = db;

        public async Task<ReminderEntity> Handle(Request request, CancellationToken cancellationToken)
        {
            
            
            ReminderEntity reminder = await _db.Reminders
                                              .AsTracking()
                                              .FirstAsync(r => r.Id == request.Reminder.Id, cancellationToken);

            reminder.ExpiresAt = request.Expiration;

            await _db.SaveChangesAsync(cancellationToken);
            return reminder;
        }
    }
}