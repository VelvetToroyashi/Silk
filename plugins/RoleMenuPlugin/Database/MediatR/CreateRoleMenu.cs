using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Remora.Results;

namespace RoleMenuPlugin.Database.MediatR
{
    public static class CreateRoleMenu
    {
        public sealed record Request(RoleMenuModel Menu) : IRequest<Result<RoleMenuModel>>;

        public sealed class Handler : IRequestHandler<Request, Result<RoleMenuModel>>
        {
            private readonly RoleMenuContext _db;
            public Handler(RoleMenuContext db) => _db = db;

            public async Task<Result<RoleMenuModel>> Handle(Request request, CancellationToken cancellationToken)
            {
                RoleMenuModel? rm = request.Menu;

                try
                {
                    _db.RoleMenus.Add(rm);
                    await _db.SaveChangesAsync(cancellationToken);

                    return Result<RoleMenuModel>.FromSuccess(rm);
                }
                catch (Exception e)
                {
                    return Result<RoleMenuModel>.FromError(new ExceptionError(e, "A role menu with the defined message ID was already present in the database."));
                }
            }
        }
    }
}