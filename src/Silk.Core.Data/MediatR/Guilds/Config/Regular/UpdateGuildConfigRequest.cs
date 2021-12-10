using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Remora.Rest.Core;
using Silk.Core.Data.Entities;

namespace Silk.Core.Data.MediatR.Guilds;

/// <summary>
///     Request for updating a <see cref="GuildConfigEntity" /> for a Guild.
/// </summary>
/// <param name="GuildId">The Id of the Guild</param>
public record UpdateGuildConfigRequest(ulong GuildId) : IRequest<GuildConfigEntity?>
{
    [Obsolete]
    public Optional<ulong>          GreetingChannelId  { get; init; }
    
    [Obsolete]
    public Optional<ulong>          VerificationRoleId { get; init; }
    
    [Obsolete]
    public Optional<GreetingOption> GreetingOption     { get; init; }
    
    [Obsolete]
    public Optional<string>         GreetingText       { get; init; }

    //TODO: Either remove this or actually implement it. It cannot remain in limbo, which it currently is.
    public List<DisabledCommandEntity>? DisabledCommands { get; init; }
}

/// <summary>
///     The default handler for <see cref="UpdateGuildConfigRequest" />.
/// </summary>
public class UpdateGuildConfigHandler : IRequestHandler<UpdateGuildConfigRequest, GuildConfigEntity?>
{
    private readonly GuildContext _db;

    public UpdateGuildConfigHandler(GuildContext db) => _db = db;

    public async Task<GuildConfigEntity?> Handle(UpdateGuildConfigRequest request, CancellationToken cancellationToken)
    {
        GuildConfigEntity? config = await _db.GuildConfigs
                                             .AsSplitQuery()
                                             .FirstOrDefaultAsync(g => g.GuildID.Value == request.GuildId, cancellationToken);

        if (request.GreetingOption.IsDefined(out GreetingOption greeting))
            config.GreetingOption = greeting;

        if (request.GreetingChannelId.IsDefined(out ulong channel))
            config.GreetingChannel = channel;

        if (request.VerificationRoleId.IsDefined(out ulong role))
            config.VerificationRole = role;

        if (request.GreetingText.IsDefined(out string? text))
            config.GreetingText = text;

        config.DisabledCommands = request.DisabledCommands ?? config.DisabledCommands;

        await _db.SaveChangesAsync(cancellationToken);

        return config;
    }
}