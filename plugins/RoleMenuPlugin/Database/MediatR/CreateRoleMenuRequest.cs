using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace RoleMenuPlugin.Database.MediatR
{
	public static class CreateRoleMenu
	{
		public sealed record Request(RoleMenuModel Menu) : IRequest;

		public sealed class Handler : IRequestHandler<Request>
		{
			private readonly RolemenuContext _db;
			public Handler(RolemenuContext db) => _db = db;

			public async Task<Unit> Handle(Request request, CancellationToken cancellationToken)
			{
				var rm = new RoleMenuModel()
				{
					MessageId = request.Menu.MessageId,
					GuildId = request.Menu.GuildId,
					Options = request.Menu.Options.Select(o => new RoleMenuOptionModel()
						{
							RoleId = o.RoleId,
							GuildId = o.GuildId,
							Description = o.Description,
							ComponentId = o.ComponentId,
							EmojiName = o.EmojiName
						})
						.ToList()
				};

				_db.RoleMenus.Add(rm);
				await _db.SaveChangesAsync(cancellationToken);

				return Unit.Value;
			}
		}
	}
}