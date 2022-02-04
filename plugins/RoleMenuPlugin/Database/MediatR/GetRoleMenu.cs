using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Remora.Results;

namespace RoleMenuPlugin.Database.MediatR
{

    public static class GetRoleMenu
    {
        /// <summary>
        ///     Retrieves a <see cref="RoleMenuModel" /> from the database in the form of a <see cref="RoleMenuModel" />
        /// </summary>
        /// <param name="MessageId">The Id of the message to grab.</param>
        public sealed record Request(ulong MessageId) : IRequest<Result<RoleMenuModel>>;

        internal sealed class Handler : IRequestHandler<Request, Result<RoleMenuModel>>
        {
            private readonly RoleMenuContext _db;
            public Handler(RoleMenuContext db) => _db = db;

            public async Task<Result<RoleMenuModel>> Handle(Request request, CancellationToken cancellationToken)
            {
                RoleMenuModel? rolemenu = await _db.RoleMenus
                                                   .AsNoTracking()
                                                   .Include(r => r.Options)
                                                   .FirstOrDefaultAsync(r => r.MessageId == request.MessageId, cancellationToken);
                
                return rolemenu is not null 
                    ? Result<RoleMenuModel>.FromSuccess(rolemenu)
                    : Result<RoleMenuModel>.FromError(new NotFoundError($"No role menu with the specified ID of {request.MessageId} was found."));
            }
        }
    }
}