using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Remora.Results;

namespace RoleMenuPlugin.Database.MediatR
{
    public static class GetGuildRoleMenusRequest
    {
        public record Request(ulong GuildId) : IRequest<Result<IEnumerable<RoleMenuModel>>>;

        [EditorBrowsable(EditorBrowsableState.Never)]
        internal class Handler : IRequestHandler<Request, Result<IEnumerable<RoleMenuModel>>>
        {
            private readonly RoleMenuContext _db;

            public Handler(RoleMenuContext db) => _db = db;

            public async ValueTask<Result<IEnumerable<RoleMenuModel>>> Handle(Request request, CancellationToken token)
            {
                List<RoleMenuModel>? results = await _db.RoleMenus
                                                        .Include(c => c.Options)
                                                        .Where(x => x.GuildId == request.GuildId)
                                                        .ToListAsync(token);

                return results.Any() ?
                    Result<IEnumerable<RoleMenuModel>>.FromSuccess(results) :
                    Result<IEnumerable<RoleMenuModel>>.FromError(new NotFoundError("No role menus have been registered for the guild."));
            }
        }
    }
}