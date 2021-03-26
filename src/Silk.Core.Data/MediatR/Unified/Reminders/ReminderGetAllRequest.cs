using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Silk.Core.Data.Models;

namespace Silk.Core.Data.MediatR.Unified.Reminders
{
    public record GetAllRemindersRequest : IRequest<IEnumerable<Reminder>>;

    public class GetAllRemindersHandler : IRequestHandler<ReminderRequest.GetAll, IEnumerable<Reminder>>
    {
        private readonly GuildContext _db;
        public GetAllRemindersHandler(GuildContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<Reminder>> Handle(ReminderRequest.GetAll request, CancellationToken cancellationToken)
        {
            return await _db.Reminders
                .Include(r => r.Owner)
                .ToListAsync(cancellationToken);
        }
    }
}