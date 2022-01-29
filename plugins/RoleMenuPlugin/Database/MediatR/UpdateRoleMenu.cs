﻿using System;
using System.Collections.Generic;
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
        public record Request(Snowflake RoleMenuID, List<RoleMenuOptionModel> Options = null) : IRequest<Result>;

        internal class Handler : IRequestHandler<Request, Result>
        {
            private readonly RoleMenuContext _db;

            public Handler(RoleMenuContext db) => _db = db;

            public async Task<Result> Handle(Request request, CancellationToken cancellationToken)
            {
                RoleMenuModel? roleMenu = await _db
                                               .RoleMenus
                                               .Include(r => r.Options)
                                               .FirstOrDefaultAsync(r => r.MessageId == request.RoleMenuID.Value, cancellationToken);

                if (roleMenu is null)
                    return Result.FromError(new NotFoundError("RoleMenu not found"));

                if (request.Options.Count is < 1 or > 25)
                    return Result.FromError(new ArgumentOutOfRangeError(nameof(request.Options), "Options must be between 1 and 25"));

                roleMenu.Options.Clear();
                roleMenu.Options.AddRange(request.Options);

                var        saved = 0;
                Exception? ex    = null;

                _db.RoleMenus.Update(roleMenu);

                try { saved = await _db.SaveChangesAsync(cancellationToken); }
                catch (Exception e) { ex = e; }

                return saved is 1
                    ? Result.FromSuccess()
                    : Result.FromError(new ExceptionError(ex!, "Unable to update RoleMenu"));
            }
        }
    }
}