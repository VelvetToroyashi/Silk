using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace RoleMenuPlugin.Database.MediatR
{
	public static class GetGuildRoleMenus
	{
		public record Request(ulong GuildId) : IRequest<IEnumerable<RoleMenuModel>>;

		internal class Handler : IRequestHandler<Request, IEnumerable<RoleMenuModel>>
		{
			private readonly RoleMenuContext _db;

			public Handler(RoleMenuContext db) => _db = db;

			public async Task<IEnumerable<RoleMenuModel>> Handle(Request request, CancellationToken token)
				=> await _db.RoleMenus.Where(x => x.GuildId == request.GuildId).ToListAsync(token);
		}
	}
}