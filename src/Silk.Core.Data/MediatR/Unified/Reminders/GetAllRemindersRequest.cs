using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Silk.Core.Data.Models;

namespace Silk.Core.Data.MediatR.Unified.Reminders
{
    /// <summary>
    ///     Request for getting all reminders.
    /// </summary>
    public record GetAllRemindersRequest : IRequest<IEnumerable<Reminder>>;

    /// <summary>
    ///     The default handler for <see cref="GetAllRemindersRequest" />.
    /// </summary>
    public class GetAllRemindersHandler : IRequestHandler<GetAllRemindersRequest, IEnumerable<Reminder>>
    {
        private readonly GuildContext _db;

        public GetAllRemindersHandler(GuildContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<Reminder>> Handle(
            GetAllRemindersRequest request,
            CancellationToken cancellationToken)
        {
            return await _db.Reminders
                .Include(r => r.Owner)
                .ToListAsync(cancellationToken);
        }
    }
}