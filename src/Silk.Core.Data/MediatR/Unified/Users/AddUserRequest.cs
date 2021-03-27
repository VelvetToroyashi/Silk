using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Silk.Core.Data.Models;

namespace Silk.Core.Data.MediatR.Unified.Users
{
    /// <summary>
    /// Request to add a user to the database.
    /// </summary>
    public record AddUserRequest(ulong GuildId, ulong UserId, UserFlag? Flags = null) : IRequest<User>;

    /// <summary>
    /// The default handler for <see cref="AddUserRequest"/>.
    /// </summary>
    public class AddUserHandler : IRequestHandler<AddUserRequest, User>
    {
        private readonly GuildContext _db;

        public AddUserHandler(GuildContext db) => _db = db;

        public async Task<User> Handle(AddUserRequest request, CancellationToken cancellationToken)
        {
            var user = new User {Id = request.UserId, GuildId = request.GuildId, Flags = request.Flags ?? UserFlag.None};
            _db.Users.Add(user);
            
            await _db.SaveChangesAsync(cancellationToken);
            return user;
        }
    }
}