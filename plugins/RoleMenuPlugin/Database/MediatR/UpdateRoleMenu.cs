using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Remora.Rest.Core;
using Remora.Results;

namespace RoleMenuPlugin.Database.MediatR
{
    public static class UpdateRoleMenu
    {
        public record Request(Snowflake RoleMenuID, IEnumerable<RoleMenuOptionModel> Options, int? MaxOptions = null) : IRequest<Result>;

        internal class Handler : IRequestHandler<Request, Result>
        {
            private readonly RoleMenuContext _db;

            public Handler(RoleMenuContext db) => _db = db;

            public async Task<Result> Handle(Request request, CancellationToken cancellationToken)
            {
                var roleMenu = await _db
                                    .RoleMenus
                                    .AsNoTracking()
                                    .Include(r => r.Options)
                                    .FirstOrDefaultAsync(r => r.MessageId == request.RoleMenuID.Value, cancellationToken);

                if (roleMenu is null)
                    return Result.FromError(new NotFoundError("RoleMenu not found"));

                if (request.Options.Count() is < 1 or > 25)
                    return Result.FromError(new ArgumentOutOfRangeError(nameof(request.Options), "Options must be between 1 and 25"));
                
                
                _db.RemoveRange(roleMenu.Options.Except(request.Options));
                
                roleMenu.MaxSelections = request.MaxOptions ?? roleMenu.MaxSelections;
                
                roleMenu.Options.Clear();
                roleMenu.Options.AddRange(request.Options);

                _db.Update(roleMenu);

                var        saved = 0;
                Exception? ex    = null;
                
                try { saved = await _db.SaveChangesAsync(cancellationToken); }
                catch (Exception e) { ex = e; }

                return saved > 0
                    ? Result.FromSuccess()
                    : Result.FromError(new ExceptionError(ex!, "Unable to update RoleMenu"));
            }
        }
    }
}