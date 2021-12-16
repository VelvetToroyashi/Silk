using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Remora.Rest.Core;
using Silk.Data.Entities;
using Silk.Data.MediatR.Users;

namespace Silk.Data.MediatR.Reminders;

/// <summary>
///     Request for creating a <see cref="ReminderEntity" />.
/// </summary>
public record CreateReminderRequest
(
    DateTime   Expiration,
    Snowflake  OwnerID,
    Snowflake  ChannelID,
    Snowflake? MessageID,
    Snowflake? GuildID,
    string?    MessageContent,
    Snowflake? ReplyID             = null,
    Snowflake? ReplyAuthorID       = null,
    string?    ReplyMessageContent = null
) : IRequest<ReminderEntity>;

/// <summary>
///     The default handler for <see cref="T:Silk.Data.MediatR.Reminders.CreateReminderRequest" />.
/// </summary>
public class CreateReminderHandler : IRequestHandler<CreateReminderRequest, ReminderEntity>
{
    private readonly GuildContext _db;

    public CreateReminderHandler(GuildContext db) => _db = db;

    public async Task<ReminderEntity> Handle(CreateReminderRequest request, CancellationToken cancellationToken)
    {
        ReminderEntity r = new()
        {
            ExpiresAt           = request.Expiration,
            CreatedAt           = DateTime.UtcNow,
            OwnerID             = request.OwnerID,
            ChannelID           = request.ChannelID,
            MessageID           = request.MessageID,
            GuildID             = request.GuildID,
            ReplyID             = request.ReplyID,
            MessageContent      = request.MessageContent,
            ReplyAuthorID       = request.ReplyAuthorID,
            ReplyMessageContent = request.ReplyMessageContent
        };

        _db.Add(r);

        await _db.SaveChangesAsync(cancellationToken);

        return r;
    }
}