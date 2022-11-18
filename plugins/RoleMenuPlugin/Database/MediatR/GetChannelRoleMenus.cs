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
    public static class GetChannelRoleMenusRequest
    {
        public record Request(ulong ChannelId) : IRequest<Result<IEnumerable<RoleMenuModel>>>;

        [EditorBrowsable(EditorBrowsableState.Never)]
        internal class Handler : IRequestHandler<Request, Result<IEnumerable<RoleMenuModel>>>
        {
            private readonly RoleMenuContext _db;

            public Handler(RoleMenuContext db) => _db = db;

            public async ValueTask<Result<IEnumerable<RoleMenuModel>>> Handle(Request request, CancellationToken cancellationToken)
            {
                List<RoleMenuModel>? results = await _db.RoleMenus
                                                        .Include(c => c.Options)
                                                        .Where(x => x.ChannelId == request.ChannelId)
                                                        .ToListAsync(cancellationToken);

                return results.Any() ?
                    Result<IEnumerable<RoleMenuModel>>.FromSuccess(results) :
                    Result<IEnumerable<RoleMenuModel>>.FromError(new NotFoundError("No role menus found for channel"));
            }
        }
    }
}