using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Silk.Core.Data.Models;

namespace Silk.Core.Data.MediatR.Handlers
{
    public class ReminderHandler
    {
        public class GetAllHandler : IRequestHandler<ReminderRequest.GetAll, IEnumerable<Reminder>>
        {
            private readonly SilkDbContext _db;
            public GetAllHandler(SilkDbContext db)
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

        public class CreateHandler : IRequestHandler<ReminderRequest.Create, Reminder>
        {
            private readonly SilkDbContext _db;
            private readonly IServiceProvider _provider;
            private readonly IMediator _mediator;
            public CreateHandler(IMediator mediator, SilkDbContext db, IServiceProvider provider)
            {
                _mediator = mediator;
                _db = db;
                _provider = provider;
            }
            
            public async Task<Reminder> Handle(ReminderRequest.Create request, CancellationToken cancellationToken)
            {
                using (var scope = _provider.CreateScope())
                {
                    var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                    _ = await mediator.Send(new UserRequest.GetOrCreate(request.GuildId, request.OwnerId), cancellationToken);
                }
                
                Reminder r = new()
                {
                    Expiration = request.Expiration,
                    CreationTime = DateTime.Now.ToUniversalTime(),
                    OwnerId = request.OwnerId,
                    ChannelId = request.ChannelId,
                    MessageId = request.MessageId,
                    GuildId = request.GuildId,
                    ReplyId = request.ReplyId,
                    MessageContent = request.MessageContent,
                    WasReply = request.WasReply,
                    ReplyAuthorId = request.ReplyAuthorId,
                    ReplyMessageContent = request.ReplyMessageContent,
                };

                _db.Add(r);
                
                await _db.SaveChangesAsync(cancellationToken);
                                
                return r;
            }
        }
        
        public class RemoveHandler : IRequestHandler<ReminderRequest.Remove>
        {
            private readonly SilkDbContext _db;
            public RemoveHandler(SilkDbContext db)
            {
                _db = db;
            }
            public async Task<Unit> Handle(ReminderRequest.Remove request, CancellationToken cancellationToken)
            {
                Reminder? reminder = await _db.Reminders.FirstOrDefaultAsync(r => r.Id == request.ReminderId, cancellationToken);
                if (reminder is not null)
                {
                    _db.Reminders.Remove(reminder);
                    try
                    {
                        await _db.SaveChangesAsync(cancellationToken);
                    }
                    // Timer timed out and it got dequeued slower than it should've. //
                    catch(DbUpdateConcurrencyException) { }

                }
                return new();
            }
        }
    }
}