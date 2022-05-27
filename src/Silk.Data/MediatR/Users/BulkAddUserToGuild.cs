using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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
    public sealed record Request(IEnumerable<UserEntity> Users, Snowflake GuildID) : IRequest<IEnumerable<UserEntity>>;

    /// <summary>
    /// The default handler for <see cref="Request" />.
    /// </summary>
    internal sealed class Handler : IRequestHandler<Request, IEnumerable<UserEntity>>
    {
        private readonly GuildContext       _db;
        private readonly IServiceProvider _services;

        private static readonly SemaphoreSlim _lock = new(1);

        public Handler(GuildContext db, IServiceProvider services)
        {
            _db            = db;
            _services = services;
        }

        public async Task<IEnumerable<UserEntity>> Handle(Request request, CancellationToken  cancellationToken)
        {
            await _lock.WaitAsync(CancellationToken.None);
            
            await BulkInsertUsersAsync(request.Users);
            
            var users = await _db.Guilds
                                 .AsNoTracking()
                                 .Where(g => g.ID == request.GuildID)
                                 .SelectMany(g => g.Users)
                                 .Select(u => u.ID)
                                 .ToListAsync(cancellationToken);
            
            var guild = await _db.Guilds.FirstAsync(g => g.ID == request.GuildID, cancellationToken);
            
            var upserting = request.Users.ExceptBy(users, u => u.ID);
            
            
            guild.Users.AddRange(upserting);
            _db.AttachRange(upserting.DistinctBy(u => u.ID));
            
            await _db.SaveChangesAsync(cancellationToken);

            _lock.Release();
            
            return request.Users;
        }

        private async Task BulkInsertUsersAsync(IEnumerable<UserEntity> users)
        {
            foreach (var user in users)
            {
                await _db.Database.ExecuteSqlRawAsync("INSERT INTO users VALUES(@p0) ON CONFLICT(id) DO NOTHING;", user.ID.Value);
            }
        }
    }
}