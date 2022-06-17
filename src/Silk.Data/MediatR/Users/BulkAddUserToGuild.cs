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
        private readonly GuildContext       _db;
        public Handler(GuildContext db) => _db = db;

        public async Task<Unit> Handle(Request request, CancellationToken  cancellationToken)
        {
            await using var trans = await _db.Database.BeginTransactionAsync(cancellationToken);
            
            var users      = await _db.Users.AsNoTracking().Select(u => u.ID).ToArrayAsync(cancellationToken);
            var usersToAdd = request.Users.ExceptBy(users, u => u.ID);

            if (usersToAdd.Any())
                _db.Users.AddRange(usersToAdd.Select(u => new UserEntity() { ID = u.ID, History = new() { new() { GuildID = request.GuildID, JoinDate = u.JoinedAt } } }));

            var guildUsers = await _db.GuildUsers
                                 .AsNoTracking()
                                 .Where(gu => gu.GuildID == request.GuildID)
                                 .Select(gu => gu.UserID)
                                 .ToArrayAsync(cancellationToken);

            var guildUsersToAdd = request.Users.ExceptBy(guildUsers, u => u.ID);
            
            Console.WriteLine($"User Count: {users.Length}, Users to add: {usersToAdd.Count()} | Guild users: {guildUsers.Length}, Guild users to add: {guildUsersToAdd.Count()}");
            
            if (guildUsersToAdd.Any())
                _db.GuildUsers.AddRange(guildUsersToAdd.Select(u => new GuildUserEntity { GuildID = request.GuildID, UserID = u.ID }));

            if (usersToAdd.Any() || guildUsersToAdd.Any())
                await _db.SaveChangesAsync(cancellationToken);

            await trans.CommitAsync(cancellationToken);
            
            return Unit.Value;
        }
    }
}