using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace RoleMenuPlugin.Database.MediatR
{
	public sealed record CreateRoleMenuRequest(RoleMenuDto Menu) : IRequest;
	
	public sealed class CreateRoleMenuHandler : IRequestHandler<CreateRoleMenuRequest>
	{
		public async Task<Unit> Handle(CreateRoleMenuRequest request, CancellationToken cancellationToken)
		{
			return Unit.Value;
		}
	}
}