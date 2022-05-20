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
    public sealed record Request(Snowflake GuildID, Snowflake UserID, UserFlag? Flags = null, DateTimeOffset? JoinedAt = null) : IRequest<Result<UserEntity>>;
    
    /// <summary>
    /// The default handler for <see cref="Request" />.
    /// </summary>
    internal sealed class Handler : IRequestHandler<Request, Result<UserEntity>>
    {
        private readonly GuildContext _db;
        public Handler(GuildContext db) => _db = db;
    
        public async Task<Result<UserEntity>> Handle(Request request, CancellationToken cancellationToken)
        {
            var user = await _db.Users
                                .Include(u => u.Guilds)
                                .Include(u => u.History)
                                .Include(u => u.Infractions)
                                .FirstOrDefaultAsync(u => u.ID == request.UserID, cancellationToken);
    
            if (user is not null)
                return user;
            
            var guild = await _db.Guilds.FirstAsync(g => g.ID == request.GuildID, cancellationToken);
            
            user = new()
            {
                ID          = request.UserID,
                Infractions = new(), 
                History     = new() { new() { GuildID = request.GuildID, JoinDate = request.JoinedAt ?? DateTimeOffset.UtcNow } }
            };
    
            _db.Users.Add(user);
            
            guild.Users.Add(user);
            
            try
            {
                await _db.SaveChangesAsync(cancellationToken);
            }
            catch (Exception e)
            {
                return Result<UserEntity>.FromError(new ExceptionError(e));
            }
    
            return user;
        }
    }
}