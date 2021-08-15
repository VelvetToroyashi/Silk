using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace RoleMenuPlugin.Database.MediatR
{
	/// <summary>
	/// Retrieves a <see cref="RoleMenuModel"/> from the database in the form of a <see cref="RoleMenuDto"/>
	/// </summary>
	/// <param name="MessageId">The Id of the message to grab.</param>
	public sealed record GetRoleMenuRequest(ulong MessageId) : IRequest<RoleMenuDto>;
	
	public sealed class GetRoleMenuHandler : IRequestHandler<GetRoleMenuRequest, RoleMenuDto>
	{
		private readonly RolemenuContext _db;
		public GetRoleMenuHandler(RolemenuContext db) => _db = db;

		public async Task<RoleMenuDto> Handle(GetRoleMenuRequest request, CancellationToken cancellationToken)
		{
			var rolemenu = await _db.RoleMenus
				.Include(r => r.Options)
				.FirstOrDefaultAsync(r => r.MessageId == request.MessageId, cancellationToken);

			if (rolemenu is null) // Not a role menu //
				return null;
			
			var dtoOptions = rolemenu.Options.Select(option => new RoleMenuOptionDto()
			{
				RoleId = option.RoleId,
				MessageId = option.MessageId,
				Description = option.Description,
				ComponentId = option.ComponentId,
				EmojiName = option.EmojiName
			});

			var dtoRoleMenu = new RoleMenuDto()
			{
				MessageId = request.MessageId,
				Options = dtoOptions.ToArray()
			};

			return dtoRoleMenu;
		}
	}
}