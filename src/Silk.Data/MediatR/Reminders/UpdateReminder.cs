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
        private readonly IDbContextFactory<GuildContext> _dbFactory;
        public Handler(IDbContextFactory<GuildContext> dbFactory) => _dbFactory = dbFactory;

        public async Task<ReminderEntity> Handle(Request request, CancellationToken cancellationToken)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            
            ReminderEntity reminder = await db.Reminders
                                              .AsTracking()
                                              .FirstAsync(r => r.Id == request.Reminder.Id, cancellationToken);

            reminder.ExpiresAt = request.Expiration;

            await db.SaveChangesAsync(cancellationToken);
            return reminder;
        }
    }
}