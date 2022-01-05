using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Rest.Core;
using Silk.Data.Entities;
using Silk.Data.MediatR.Users;

namespace Silk.Data.MediatR.Reminders;

/// <summary>
///     Request for creating a <see cref="ReminderEntity" />.
/// </summary>
public record CreateReminderRequest
(
    DateTimeOffset Expiration,
    Snowflake      OwnerID,
    Snowflake      ChannelID,
    Snowflake?     MessageID,
    Snowflake?     GuildID,
    string?        MessageContent,
    Snowflake?     ReplyID             = null,
    Snowflake?     ReplyAuthorID       = null,
    string?        ReplyMessageContent = null
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
        var reminder = new ReminderEntity()
        {
            ExpiresAt           = request.Expiration,
            CreatedAt           = DateTimeOffset.UtcNow,
            OwnerID             = request.OwnerID,
            ChannelID           = request.ChannelID,
            MessageID           = request.MessageID,
            GuildID             = request.GuildID,
            ReplyMessageID      = request.ReplyID,
            MessageContent      = request.MessageContent,
            ReplyAuthorID       = request.ReplyAuthorID,
            ReplyMessageContent = request.ReplyMessageContent,
            IsPrivate           = request.GuildID is null,
            IsReply             = request.ReplyID is not null,
        };

        _db.Add(reminder);

        await _db.SaveChangesAsync(cancellationToken);

        return reminder;
    }
}