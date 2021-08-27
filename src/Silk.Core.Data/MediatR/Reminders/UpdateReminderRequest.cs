using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Silk.Core.Data.Entities;

namespace Silk.Core.Data.MediatR.Reminders
{
    public record UpdateReminderRequest(ReminderEntity Reminder, DateTime Expiration) : IRequest<ReminderEntity>;

    public class UpdateReminderHandler : IRequestHandler<UpdateReminderRequest, ReminderEntity>
    {
        private readonly GuildContext _db;
        public UpdateReminderHandler(GuildContext db)
        {
            _db = db;
        }

        public async Task<ReminderEntity> Handle(UpdateReminderRequest request, CancellationToken cancellationToken)
        {
            ReminderEntity reminder = await _db.Reminders.FirstAsync(r => r.Id == request.Reminder.Id, cancellationToken);
            reminder.Expiration = request.Expiration;
            await _db.SaveChangesAsync(cancellationToken);
            return reminder;
        }
    }
}