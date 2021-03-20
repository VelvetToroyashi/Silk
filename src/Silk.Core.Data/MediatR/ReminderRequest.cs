using System;
using System.Collections.Generic;
using MediatR;
using Silk.Core.Data.Models;

namespace Silk.Core.Data.MediatR
{
    public class ReminderRequest
    {
        public record GetAll : IRequest<IEnumerable<Reminder>>;

        public record Create(
            DateTime Expiration, ulong OwnerId,
            ulong ChannelId, ulong MessageId, ulong GuildId,
            string MessageContent, bool WasReply, ulong? ReplyId = null,
            ulong? ReplyAuthorId = null, string? ReplyMessageContent = null) : IRequest<Reminder>;
        
        public record Remove(int ReminderId) : IRequest;
    }
}