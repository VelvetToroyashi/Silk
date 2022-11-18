using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Remora.Rest.Core;
using Silk.Data.Entities;

namespace Silk.Data.MediatR.Reminders;

public static class GetRemindersForUser
{
    public record Request(Snowflake userID) : IRequest<IEnumerable<ReminderEntity>>;
    
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal class Handler : IRequestHandler<Request, IEnumerable<ReminderEntity>>
    {
        private readonly GuildContext _db;
        public Handler(GuildContext db) => _db = db;

        public async ValueTask<IEnumerable<ReminderEntity>> Handle(Request request, CancellationToken cancellationToken)
        {
            
            
            return await _db.Reminders.Where(x => x.OwnerID == request.userID).ToListAsync(cancellationToken);
        }
    }
}