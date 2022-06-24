using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Remora.Rest.Core;
using Remora.Results;
using Silk.Data.Entities;

namespace Silk.Data.MediatR.Users;

public static class GetOrCreateUser
{
    /// <summary>
    /// Request to get a user from the database, or creates one if it does not exist.
    /// </summary>
    public sealed record Request(Snowflake GuildID, Snowflake UserID, DateTimeOffset JoinedAt = default) : IRequest<Result<UserEntity>>;
    
    /// <summary>
    /// The default handler for <see cref="Request" />.
    /// </summary>
    internal sealed class Handler : IRequestHandler<Request, Result<UserEntity>>, IAsyncDisposable
    {
        private readonly GuildContext _db;
        public Handler(GuildContext db) => _db = db;
    
        public async Task<Result<UserEntity>> Handle(Request request, CancellationToken cancellationToken)
        {
            await _db.Upsert(new UserEntity { ID = request.UserID })
                     .NoUpdate()
                     .RunAsync(cancellationToken);

            await _db.Upsert(new UserHistoryEntity { UserID = request.UserID, GuildID = request.GuildID, Date = request.JoinedAt, IsJoin = true })
                     .On(u => new { u.UserID, u.GuildID, u.Date })
                     .NoUpdate()
                     .RunAsync(cancellationToken);
            
            await _db.Upsert(new GuildUserEntity { UserID = request.UserID, GuildID = request.GuildID })
                     .NoUpdate()
                     .RunAsync(cancellationToken);
            
            var user = await _db.Users
                                .AsNoTracking()
                                .Include(u => u.Guilds)
                                .Include(u => u.History)
                                .Include(u => u.Infractions)
                                .FirstOrDefaultAsync(u => u.ID == request.UserID, cancellationToken);
            
            return user;
        }
        
        public ValueTask DisposeAsync() => _db.DisposeAsync();
    }
}