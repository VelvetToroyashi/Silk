using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace RoleMenuPlugin.Database.MediatR
{
	public sealed record CreateRoleMenuRequest(RoleMenuDto Menu) : IRequest;
	
	public sealed class CreateRoleMenuHandler : IRequestHandler<CreateRoleMenuRequest>
	{
		private readonly RolemenuContext _db;
		public CreateRoleMenuHandler(RolemenuContext db) => _db = db;

		public async Task<Unit> Handle(CreateRoleMenuRequest request, CancellationToken cancellationToken)
		{
			var rm = new RoleMenuModel()
			{
				MessageId = request.Menu.MessageId,
				Options = request.Menu.Options.Select(o => new RoleMenuOptionModel()
				{
					RoleId = o.RoleId,
					Description = o.Description,
					ComponentId = o.ComponentId,
					EmojiName = o.EmojiName
				}).ToList()
			};

			_db.RoleMenus.Add(rm);
			await _db.SaveChangesAsync(cancellationToken);
			
			return Unit.Value;
		}
	}
}