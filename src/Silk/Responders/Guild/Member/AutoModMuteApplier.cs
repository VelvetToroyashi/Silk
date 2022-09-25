using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Gateway.Responders;
using Remora.Results;
using Silk.Services.Interfaces;
using Silk.Shared.Constants;

namespace Silk.Responders;

/// <summary>
///     An AutoMod feature that automatically re-applies mutes when members rejoin a guild.
/// </summary>
public sealed class AutoModMuteApplier : IResponder<IGuildMemberAdd>
{
    private readonly IInfractionService          _infractions;
    private readonly ILogger<AutoModMuteApplier> _logger;
    private readonly IDiscordRestUserAPI         _users;
    public AutoModMuteApplier(IInfractionService infractions, ILogger<AutoModMuteApplier> logger, IDiscordRestUserAPI users)
    {
        _users       = users;
        _logger      = logger;
        _infractions = infractions;
    }

    public async Task<Result> RespondAsync(IGuildMemberAdd gatewayEvent, CancellationToken ct = default)
    {
        if (!gatewayEvent.User.IsDefined())
            return Result.FromSuccess(); // ??? 

        var guild = gatewayEvent.GuildID;
        var member = gatewayEvent.User.Value.ID;

        var isMuted = await _infractions.IsMutedAsync(member, guild);
        
        if (!isMuted)
            return Result.FromSuccess();

        var automodRes = await _users.GetCurrentUserAsync(ct);

        if (!automodRes.IsSuccess)
            return Result.FromError(automodRes.Error);

        var automod = automodRes.Entity.ID;
        
        await _infractions.MuteAsync(member, guild, automod, "Re-applied active mute on join.");

        return Result.FromSuccess();
    }
}