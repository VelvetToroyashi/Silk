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
    internal sealed class Handler : IRequestHandler<Request>
    {
        private readonly GuildContext _db;
        private readonly IMediator    _mediator;
        
        public Handler(GuildContext db, IMediator mediator)
        {
            _db       = db;
            _mediator = mediator;
        }

        public async Task<Unit> Handle(Request request, CancellationToken  cancellationToken)
        {
            await _mediator.Send(new GetOrCreateGuild.Request(request.GuildID, "s!"), cancellationToken);
            
            await using var trans = await _db.Database.BeginTransactionAsync(cancellationToken);

            var users      = request.Users.Select(u => new UserEntity() { ID          = u.ID, History = new() { new() { UserID = u.ID, GuildID = request.GuildID, JoinDate = u.JoinedAt } } });
            var guildUsers = request.Users.Select(u => new GuildUserEntity() { UserID = u.ID, GuildID = request.GuildID });
            
            await _db.Users.UpsertRange(users).NoUpdate().RunAsync(cancellationToken);
            await _db.GuildUsers.UpsertRange(guildUsers).NoUpdate().RunAsync(cancellationToken);

            await trans.CommitAsync(cancellationToken);
            
            return Unit.Value;
        }
    }
}