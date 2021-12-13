using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Remora.Rest.Core;
using Silk.Core.Data.Entities;

namespace Silk.Core.Data.MediatR.Users;

/// <summary>
///     Request to add a user to the database.
/// </summary>
public record AddUserRequest(Snowflake GuildID, Snowflake UserID, UserFlag? Flags = null, DateTimeOffset? JoinedAt = null) : IRequest<UserEntity>;

/// <summary>
///     The default handler for <see cref="AddUserRequest" />.
/// </summary>
public class AddUserHandler : IRequestHandler<AddUserRequest, UserEntity>
{
    private readonly GuildContext _db;
    public AddUserHandler(GuildContext db) => _db = db;

    public async Task<UserEntity> Handle(AddUserRequest request, CancellationToken cancellationToken)
    {
        var user = new UserEntity
        {
            ID      = request.UserID,
            GuildID = request.GuildID,
            Flags   = request.Flags ?? UserFlag.None,
            History = new() { JoinDate = request.JoinedAt ?? DateTimeOffset.UtcNow }
        };

        _db.Users.Add(user);

        await _db.SaveChangesAsync(cancellationToken);
        return user;
    }
}