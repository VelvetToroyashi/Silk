using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Remora.Results;

namespace RoleMenuPlugin.Database.MediatR
{
	public static class GetChannelRoleMenusRequest
	{
		public record Request(ulong ChannelId) : IRequest<Result<IEnumerable<RoleMenuModel>>>;

		internal class Handler : IRequestHandler<Request, Result<IEnumerable<RoleMenuModel>>>
		{
			private readonly RoleMenuContext _db;

			public Handler(RoleMenuContext db) => _db = db;

			public async Task<Result<IEnumerable<RoleMenuModel>>> Handle(Request request, CancellationToken cancellationToken)
			{
				var results = await _db.RoleMenus.Where(x => x.ChannelId == request.ChannelId).ToListAsync(cancellationToken);

				return results.Any() ?
					Result<IEnumerable<RoleMenuModel>>.FromSuccess(results) :
					Result<IEnumerable<RoleMenuModel>>.FromError(new NotFoundError("No role menus found for channel"));
			}
		}
	}
}