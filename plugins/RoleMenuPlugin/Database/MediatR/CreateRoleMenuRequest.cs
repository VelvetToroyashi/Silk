using MediatR;

namespace RoleMenuPlugin.Database.MediatR
{
	public sealed record CreateRoleMenuRequest(RoleMenuDto Menu) : IRequest;
}