using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Silk.Core.Data.MediatR.Users;
using Silk.Core.Data.Models;

namespace Silk.Core.Data.MediatR.Reminders
{
    /// <summary>
    ///     Request for creating a <see cref="Reminder" />.
    /// </summary>
    public record CreateReminderRequest(
        DateTime Expiration, ulong OwnerId,
        ulong ChannelId, ulong? MessageId, ulong? GuildId,
        string? MessageContent, bool WasReply,
        ReminderType Type, ulong? ReplyId = null,
        ulong? ReplyAuthorId = null, string? ReplyMessageContent = null) : IRequest<Reminder>;

    /// <inheritdoc />
    /// <summary>
    ///     The default handler for <see cref="T:Silk.Core.Data.MediatR.Reminders.CreateReminderRequest" />.
    /// </summary>
    public class CreateReminderHandler : IRequestHandler<CreateReminderRequest, Reminder>
    {
        private readonly GuildContext _db;
        private readonly IMediator _mediator;

        public CreateReminderHandler(IMediator mediator, GuildContext db)
        {
            _mediator = mediator;
            _db = db;
        }

        public async Task<Reminder> Handle(CreateReminderRequest request, CancellationToken cancellationToken)
        {
            if (request.MessageId is not 0 or null)
                await _mediator.Send(new GetOrCreateUserRequest(request.GuildId.Value, request.OwnerId), cancellationToken);

            Reminder r = new()
            {
                Expiration = request.Expiration,
                CreationTime = DateTime.UtcNow,
                OwnerId = request.OwnerId,
                ChannelId = request.ChannelId,
                MessageId = request.MessageId ?? 0,
                GuildId = request.GuildId ?? 0,
                Type = request.Type,
                ReplyId = request.ReplyId,
                MessageContent = request.MessageContent,
                WasReply = request.WasReply,
                ReplyAuthorId = request.ReplyAuthorId,
                ReplyMessageContent = request.ReplyMessageContent
            };

            _db.Add(r);

            await _db.SaveChangesAsync(cancellationToken);

            return r;
        }
    }
}