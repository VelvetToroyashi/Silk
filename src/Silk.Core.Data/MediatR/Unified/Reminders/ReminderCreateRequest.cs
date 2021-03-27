using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Silk.Core.Data.MediatR.Unified.Users;
using Silk.Core.Data.Models;

namespace Silk.Core.Data.MediatR.Unified.Reminders
{
    /// <summary>
    /// Create a <see cref="Reminder"/>.
    /// </summary>
    public record ReminderCreateRequest(
        DateTime Expiration, ulong OwnerId,
        ulong ChannelId, ulong MessageId, ulong GuildId,
        string MessageContent, bool WasReply, ulong? ReplyId = null,
        ulong? ReplyAuthorId = null, string? ReplyMessageContent = null) : IRequest<Reminder>;

    /// <summary>
    /// The default handler for <see cref="ReminderCreateRequest"/>.
    /// </summary>
    public class ReminderCreateHandler : IRequestHandler<ReminderCreateRequest, Reminder>
    {
        private readonly GuildContext _db;
        private readonly IMediator _mediator;

        public ReminderCreateHandler(IMediator mediator, GuildContext db)
        {
            _mediator = mediator;
            _db = db;
        }

        public async Task<Reminder> Handle(ReminderCreateRequest request, CancellationToken cancellationToken)
        {
            await _mediator.Send(new UserGetOrCreateRequest(request.GuildId, request.OwnerId), cancellationToken);

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
}