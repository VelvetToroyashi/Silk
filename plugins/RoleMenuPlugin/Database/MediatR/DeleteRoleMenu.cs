using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Remora.Results;

namespace RoleMenuPlugin.Database.MediatR
{
    public static class DeleteRoleMenu
    {
        public record Request(ulong RoleMenuId) : IRequest<Result>;

        internal class Handler : IRequestHandler<Request, Result>
        {
            private readonly RoleMenuContext _context;

            public Handler(RoleMenuContext context) => _context = context;

            public async Task<Result> Handle(Request request, CancellationToken cancellationToken)
            {
                var entity = await _context
                                  .RoleMenus
                                  .Include(r => r.Options)
                                  .FirstOrDefaultAsync(r => r.MessageId == request.RoleMenuId, cancellationToken);

                if (entity == null)
                    return Result.FromError(new NotFoundError());

                _context.RoleMenus.Remove(entity);

                await _context.SaveChangesAsync(cancellationToken);

                return Result.FromSuccess();
            }
        }
    }
}