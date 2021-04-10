using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Silk.Core.Data.Models;

namespace Silk.Core.Data.MediatR.Unified.Reminders
{

    public record UpdateReminderRequest(Reminder Reminder, DateTime Expiration) : IRequest<Reminder>;

    public class UpdateReminderHandler : IRequestHandler<UpdateReminderRequest, Reminder>
    {
        private readonly GuildContext _db;
        public UpdateReminderHandler(GuildContext db)
        {
            _db = db;
        }

        public async Task<Reminder> Handle(UpdateReminderRequest request, CancellationToken cancellationToken)
        {
            Reminder reminder = await _db.Reminders.FirstAsync(r => r.Id == request.Reminder.Id, cancellationToken);
            reminder.Expiration = request.Expiration;
            await _db.SaveChangesAsync(cancellationToken);
            return reminder;
        }
    }
}