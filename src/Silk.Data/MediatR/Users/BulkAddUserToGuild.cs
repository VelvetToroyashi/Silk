using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Remora.Rest.Core;
using Silk.Data.Entities;
using Silk.Data.MediatR.Guilds;

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
    internal sealed class Handler : IRequestHandler<Request>, IAsyncDisposable
    {
        private readonly GuildContext _db;
        public Handler(GuildContext db) => _db = db;

        public async Task<Unit> Handle(Request request, CancellationToken  cancellationToken)
        {
            var users      = request.Users.Select(u => new UserEntity()      { ID     = u.ID, History = { new() { UserID = u.ID, GuildID = request.GuildID, JoinDate = u.JoinedAt } } });
            var guildUsers = request.Users.Select(u => new GuildUserEntity() { UserID = u.ID, GuildID = request.GuildID });
            
            await _db.Users.UpsertRange(users).On(u => u.ID).NoUpdate().RunAsync(cancellationToken);
            await _db.GuildUsers.UpsertRange(guildUsers).On(gu => new { gu.UserID, gu.GuildID }).NoUpdate().RunAsync(cancellationToken);
            
            return Unit.Value;
        }
        
        public ValueTask DisposeAsync() => _db.DisposeAsync();
    }
}