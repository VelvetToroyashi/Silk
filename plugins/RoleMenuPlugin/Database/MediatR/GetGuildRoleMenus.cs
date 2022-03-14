using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Remora.Results;

namespace RoleMenuPlugin.Database.MediatR
{
    public static class GetGuildRoleMenusRequest
    {
        public record Request(ulong GuildId) : IRequest<Result<IEnumerable<RoleMenuModel>>>;

        internal class Handler : IRequestHandler<Request, Result<IEnumerable<RoleMenuModel>>>
        {
            private readonly RoleMenuContext _db;

            public Handler(RoleMenuContext db) => _db = db;

            public async Task<Result<IEnumerable<RoleMenuModel>>> Handle(Request request, CancellationToken token)
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