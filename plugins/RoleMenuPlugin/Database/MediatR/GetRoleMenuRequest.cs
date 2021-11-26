using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace RoleMenuPlugin.Database.MediatR
{

	public static class GetRoleMenu
	{
		/// <summary>
		/// Retrieves a <see cref="RoleMenuModel"/> from the database in the form of a <see cref="RoleMenuModel"/>
		/// </summary>
		/// <param name="MessageId">The Id of the message to grab.</param>
		public sealed record Request(ulong MessageId) : IRequest<RoleMenuModel>;

		public sealed class Handler : IRequestHandler<Request, RoleMenuModel>
		{
			private readonly RoleMenuContext _db;
			public Handler(RoleMenuContext db) => _db = db;

			public async Task<RoleMenuModel> Handle(Request request, CancellationToken cancellationToken)
			{
				var rolemenu = await _db.RoleMenus
					.Include(r => r.Options)
					.FirstOrDefaultAsync(r => r.MessageId == request.MessageId, cancellationToken);

				if (rolemenu is null) // Not a role menu //
					return null;

				return rolemenu;
			}
		}
	}
}