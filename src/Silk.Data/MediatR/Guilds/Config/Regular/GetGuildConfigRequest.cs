﻿using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Remora.Rest.Core;
using Silk.Data.Entities;

namespace Silk.Data.MediatR.Guilds;

/// <summary>
///     Request for getting the <see cref="GuildConfigEntity" /> for the Guild.
/// </summary>
/// <param name="GuildId">The Id of the Guild</param>
public record GetGuildConfigRequest(Snowflake GuildId) : IRequest<GuildConfigEntity?>;

/// <summary>
///     The default handler for <see cref="GetGuildConfigRequest" />.
/// </summary>
public class GetGuildConfigHandler : IRequestHandler<GetGuildConfigRequest, GuildConfigEntity?>
{
    private readonly GuildContext _db;
    public GetGuildConfigHandler(GuildContext db) => _db = db;

    public async Task<GuildConfigEntity?> Handle(GetGuildConfigRequest request, CancellationToken cancellationToken)
    {
        GuildConfigEntity? config = await _db.GuildConfigs
                                             .Include(g => g.Greetings)
                                             .Include(c => c.DisabledCommands)
                                              //.Include(c => c.BlackListedWords)
                                             .AsSplitQuery()
                                             .FirstOrDefaultAsync(g => g.GuildID == request.GuildId, cancellationToken);

        return config;
    }
}