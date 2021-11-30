using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Silk.Core.Data.Entities;
using Silk.Core.Data.MediatR.Users;

namespace Silk.Core.Data.MediatR.Reminders
{
	/// <summary>
	///     Request for creating a <see cref="ReminderEntity" />.
	/// </summary>
	public record CreateReminderRequest(
        DateTime     Expiration,           ulong   OwnerId,
        ulong        ChannelId,            ulong?  MessageId, ulong? GuildId,
        string?      MessageContent,       bool    WasReply,
        ReminderType Type,                 ulong?  ReplyId             = null,
        ulong?       ReplyAuthorId = null, string? ReplyMessageContent = null) : IRequest<ReminderEntity>;

	/// <summary>
	///     The default handler for <see cref="T:Silk.Core.Data.MediatR.Reminders.CreateReminderRequest" />.
	/// </summary>
	public class CreateReminderHandler : IRequestHandler<CreateReminderRequest, ReminderEntity>
    {
        private readonly GuildContext _db;
        private readonly IMediator    _mediator;

        public CreateReminderHandler(IMediator mediator, GuildContext db)
        {
            _mediator = mediator;
            _db = db;
        }

        public async Task<ReminderEntity> Handle(CreateReminderRequest request, CancellationToken cancellationToken)
        {
            if (request.MessageId is not 0 or null)
                await _mediator.Send(new GetOrCreateUserRequest(request.GuildId.Value, request.OwnerId), cancellationToken);

            ReminderEntity r = new()
            {
                Expiration = request.Expiration,
                CreationTime = DateTime.UtcNow,
                OwnerId = request.OwnerId,
                ChannelId = request.ChannelId,
                MessageId = request.MessageId ?? 0,
                GuildId = request.GuildId     ?? 0,
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