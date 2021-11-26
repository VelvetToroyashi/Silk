using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace RoleMenuPlugin.Database.MediatR
{
	public static class GetChannelRoleMenu
	{
		public record Request(ulong ChannelId) : IRequest<IEnumerable<RoleMenuModel>>;

		internal class Handler : IRequestHandler<Request, IEnumerable<RoleMenuModel>>
		{
			private readonly RoleMenuContext _db;

			public Handler(RoleMenuContext db) => _db = db;

			public async Task<IEnumerable<RoleMenuModel>> Handle(Request request, CancellationToken cancellationToken)
				=> await _db.RoleMenus.Where(x => x.ChannelId == request.ChannelId).ToListAsync(cancellationToken);
		}
	}
}