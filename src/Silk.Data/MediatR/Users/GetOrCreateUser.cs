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
    internal sealed class Handler : IRequestHandler<Request, Result<UserEntity>>
    {
        private readonly IDbContextFactory<GuildContext> _dbFactory;
        public Handler(IDbContextFactory<GuildContext> dbFactory) => _dbFactory = dbFactory;

        public async Task<Result<UserEntity>> Handle(Request request, CancellationToken cancellationToken)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            
            await db.Upsert(new UserEntity { ID = request.UserID })
                     .NoUpdate()
                     .RunAsync(cancellationToken);

            await db.Upsert(new UserHistoryEntity { UserID = request.UserID, GuildID = request.GuildID, Date = request.JoinedAt, IsJoin = true })
                     .On(u => new { u.UserID, u.GuildID, u.Date })
                     .NoUpdate()
                     .RunAsync(cancellationToken);
            
            await db.Upsert(new GuildUserEntity { UserID = request.UserID, GuildID = request.GuildID })
                     .NoUpdate()
                     .RunAsync(cancellationToken);
            
            var user = await db.Users
                                .AsNoTracking()
                                .FirstOrDefaultAsync(u => u.ID == request.UserID, cancellationToken);
            
            return user;
        }
    }
}