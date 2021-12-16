using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Remora.Rest.Core;
using Remora.Results;
using Silk.Data.Entities;

namespace Silk.Data.MediatR.Guilds;

/// <summary>
///     Request for updating a <see cref="GuildConfigEntity" /> for a Guild.
/// </summary>
/// <param name="GuildId">The Id of the Guild</param>
public record UpdateGuildConfigRequest(Snowflake GuildId) : IRequest<Result>
{
    //TODO: Either remove this or actually implement it. It cannot remain in limbo, which it currently is.
    public List<DisabledCommandEntity>? DisabledCommands { get; init; }
}

/// <summary>
///     The default handler for <see cref="UpdateGuildConfigRequest" />.
/// </summary>
public class UpdateGuildConfigHandler : IRequestHandler<UpdateGuildConfigRequest, Result>
{
    private readonly GuildContext _db;

    public UpdateGuildConfigHandler(GuildContext db) => _db = db;

    public async Task<Result> Handle(UpdateGuildConfigRequest request, CancellationToken cancellationToken)
    {
        GuildConfigEntity? config = await _db.GuildConfigs
                                             .AsSplitQuery()
                                             .FirstOrDefaultAsync(g => g.GuildID == request.GuildId, cancellationToken);
        if (config is null)
            return Result.FromError(new NotFoundError());

        config.DisabledCommands = request.DisabledCommands ?? config.DisabledCommands;

        await _db.SaveChangesAsync(cancellationToken);

        return Result.FromSuccess();
    }
}