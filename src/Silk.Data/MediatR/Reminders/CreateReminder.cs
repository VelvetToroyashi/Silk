using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Remora.Rest.Core;
using Silk.Data.Entities;

namespace Silk.Data.MediatR.Reminders;

public static class CreateReminder
{
    /// <summary>
    /// Request for creating a <see cref="ReminderEntity" />.
    /// </summary>
    public sealed record Request
    (
        DateTimeOffset Expiration,
        Snowflake      OwnerID,
        Snowflake      ChannelID,
        Snowflake?     MessageID,
        Snowflake?     GuildID,
        string?        MessageContent,
        Snowflake?     ReplyID             = null,
        Snowflake?     ReplyAuthorID       = null,
        string?        ReplyMessageContent = null,
        bool           IsSilent            = false
    ) : IRequest<ReminderEntity>;

    /// <summary>
    /// The default handler for <see cref="T:Silk.Data.MediatR.Reminders.Request" />.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal sealed class Handler : IRequestHandler<Request, ReminderEntity>
    {
        private readonly IDbContextFactory<GuildContext> _dbFactory;

        public Handler(IDbContextFactory<GuildContext> dbFactory) 
            => _dbFactory = dbFactory;

        public async ValueTask<ReminderEntity> Handle(Request request, CancellationToken cancellationToken)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            
            var reminder = new ReminderEntity
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
                IsPrivate           = request.MessageID is null || request.GuildID is null,
                IsReply             = request.ReplyID is not null,
                IsQuiet             = request.IsSilent
            };

            db.Add(reminder);

            await db.SaveChangesAsync(cancellationToken);

            return reminder;
        }
    }
}