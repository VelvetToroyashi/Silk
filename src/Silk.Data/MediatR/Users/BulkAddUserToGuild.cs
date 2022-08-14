using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Remora.Rest.Core;
using Silk.Data.Entities;

namespace Silk.Data.MediatR.Users;

public static class BulkAddUserToGuild
{
    /// <summary>
    /// Request for adding users to the database en masse.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is not intended to be used with multi-guild updates as
    /// upon failure, the first element of the user collection is picked, and that Guild Id
    /// is used to query the users that are already inserted into the database to refine insertion
    /// queries. Validation should be done outside to ensure no duplicate users exist, as a slow
    /// branch will be taken if bulk-inserting fails.
    /// </para>
    /// </remarks>
    public sealed record Request(IEnumerable<(Snowflake ID, DateTimeOffset JoinedAt)> Users, Snowflake GuildID) : IRequest;

    /// <summary>
    /// The default handler for <see cref="Request" />.
    /// </summary>
    internal sealed class Handler : IRequestHandler<Request>
    {
        private readonly IDbContextFactory<GuildContext> _dbFactory;
        public Handler(IDbContextFactory<GuildContext> dbFactory) => _dbFactory = dbFactory;

        public async Task<Unit> Handle(Request request, CancellationToken  cancellationToken)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            
            var users         = request.Users.Select(u => new UserEntity
                                                         { ID       = u.ID });
            var guildUsers    = request.Users.Select(u => new GuildUserEntity
                                                         { UserID   = u.ID, GuildID = request.GuildID });
            var userHistories = request.Users.Select(u => new UserHistoryEntity
                                                         { UserID = u.ID, GuildID = request.GuildID, Date = u.JoinedAt, IsJoin = true });
            
            await db.Users.UpsertRange(users).On(u => u.ID).NoUpdate().RunAsync(cancellationToken);
            await db.GuildUsers.UpsertRange(guildUsers).On(gu => new { gu.UserID, gu.GuildID }).NoUpdate().RunAsync(cancellationToken);
            await db.Histories.UpsertRange(userHistories).On(u => new { u.UserID, u.GuildID, JoinDate = u.Date }).NoUpdate().RunAsync(cancellationToken);
            
            return Unit.Value;
        }
    }
}