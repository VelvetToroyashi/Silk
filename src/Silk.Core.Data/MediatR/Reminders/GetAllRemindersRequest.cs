using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Silk.Core.Data.Entities;

namespace Silk.Core.Data.MediatR.Reminders;

/// <summary>
///     Request for getting all reminders.
/// </summary>
public sealed record GetAllRemindersRequest : IRequest<IEnumerable<ReminderEntity>>;

/// <summary>
///     The default handler for <see cref="GetAllRemindersRequest" />.
/// </summary>
public sealed class GetAllRemindersHandler : IRequestHandler<GetAllRemindersRequest, IEnumerable<ReminderEntity>>
{
    private readonly GuildContext _db;
    public GetAllRemindersHandler(GuildContext db) => _db = db;

    public async Task<IEnumerable<ReminderEntity>> Handle(GetAllRemindersRequest request, CancellationToken cancellationToken) 
        => await _db.Reminders.ToListAsync(cancellationToken);
}