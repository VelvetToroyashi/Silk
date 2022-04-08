using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Remora.Rest.Core;
using Silk.Data.Entities.Channels;

namespace Silk.Data.MediatR.Channels;

public static class LockChannel
{
    public record Request(Snowflake ChannelID, Snowflake GuildID, Snowflake UserID, IEnumerable<Snowflake> LockedRoles, DateTimeOffset? UnlocksAt, string reason) : IRequest<ChannelLockEntity>;
    
    internal class Handler : IRequestHandler<Request, ChannelLockEntity>
    {
        private readonly GuildContext _db;
        
        public Handler(GuildContext db) => _db = db;

        public async Task<ChannelLockEntity> Handle(Request request, CancellationToken cancellationToken)
        {
            var existing = await _db.ChannelLocks.FirstOrDefaultAsync(c => c.ChannelID == request.ChannelID, cancellationToken);

            var isNew = existing is null;
            
            existing ??= new()
            {
                LockedRoles = request.LockedRoles.ToArray(),
                ChannelID   = request.ChannelID,
                GuildID     = request.GuildID,
                UserID      = request.UserID,
                Reason      = request.reason
            };
            
            existing.UnlocksAt = request.UnlocksAt;

            if (isNew)
                _db.ChannelLocks.Add(existing);
            
            await _db.SaveChangesAsync(cancellationToken);

            return existing;
        }
    }
}