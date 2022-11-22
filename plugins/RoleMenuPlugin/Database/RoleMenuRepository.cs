using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Remora.Results;

namespace RoleMenuPlugin.Database;

public class RoleMenuRepository
{
    private readonly RoleMenuContext _db;
    public RoleMenuRepository(RoleMenuContext db) => _db = db;

    public async Task<Result<RoleMenuModel>> CreateRoleMenuAsync(RoleMenuModel rm, CancellationToken ct = default)
    {
        try
        {
            _db.RoleMenus.Add(rm);
            await _db.SaveChangesAsync(ct);

            return Result<RoleMenuModel>.FromSuccess(rm);
        }
        catch (Exception e)
        {
            return Result<RoleMenuModel>.FromError(new ExceptionError(e, "A role menu with the defined message ID was already present in the database."));
        } 
    }

    public async Task<Result> DeleteRoleMenuAsync(ulong roleMenuID, CancellationToken ct = default)
    {
        var entity = await _db
                          .RoleMenus
                          .Include(r => r.Options)
                          .FirstOrDefaultAsync(r => r.MessageId == roleMenuID, ct);

        if (entity == null)
            return Result.FromError(new NotFoundError());

        _db.RoleMenus.Remove(entity);

        await _db.SaveChangesAsync(ct);

        return Result.FromSuccess();
    }

    public async Task<Result<IEnumerable<RoleMenuModel>>> GetChannelRoleMenusAsync(ulong channelID, CancellationToken ct = default)
    {
        List<RoleMenuModel>? results = await _db.RoleMenus
                                                .Include(c => c.Options)
                                                .Where(x => x.ChannelId == channelID)
                                                .ToListAsync(ct);

        return results.Any() ?
            Result<IEnumerable<RoleMenuModel>>.FromSuccess(results) :
            Result<IEnumerable<RoleMenuModel>>.FromError(new NotFoundError("No role menus found for channel"));
    }

    public async Task<Result> UpdateRoleMenuAsync(ulong roleMenuID, IEnumerable<RoleMenuOptionModel> options, int? maxOptions = null, string? description = null, CancellationToken ct = default)
    {
        var roleMenu = await _db
                            .RoleMenus
                            .AsNoTracking()
                            .Include(r => r.Options)
                            .FirstOrDefaultAsync(r => r.MessageId == roleMenuID, ct);

        if (roleMenu is null)
            return Result.FromError(new NotFoundError("RoleMenu not found"));
                
        _db.RemoveRange(roleMenu.Options.Except(options));
                
        roleMenu.Description   = description ?? roleMenu.Description;
        roleMenu.MaxSelections = maxOptions  ?? roleMenu.MaxSelections;
                
        roleMenu.Options.Clear();
        roleMenu.Options.AddRange(options);

        _db.Update(roleMenu);

        var        saved = 0;
        Exception? ex    = null;
                
        try { saved = await _db.SaveChangesAsync(ct); }
        catch (Exception e) { ex = e; }

        return saved > 0
            ? Result.FromSuccess()
            : Result.FromError(new ExceptionError(ex!, "Unable to update RoleMenu"));
    }

    public async Task<Result<RoleMenuModel>> GetRoleMenuAsync(ulong messageID, CancellationToken ct = default)
    {
        RoleMenuModel? rolemenu = await _db.RoleMenus
                                           .AsNoTracking()
                                           .Include(r => r.Options)
                                           .FirstOrDefaultAsync(r => r.MessageId == messageID, ct);
                
        return rolemenu is not null 
            ? Result<RoleMenuModel>.FromSuccess(rolemenu)
            : Result<RoleMenuModel>.FromError(new NotFoundError($"No role menu with the specified ID of {messageID} was found."));
    }
}