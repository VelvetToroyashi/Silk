using System.Threading;
using System.Threading.Tasks;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Rest.Core;
using Remora.Results;

namespace Silk.Extensions.Remora;

/// <summary>
/// A collection of extension methods for <see cref="IDiscordRestGuildAPI"/>
/// </summary>
public static class DiscordRestGuildAPIExtensions
{
    /// <summary>
    /// Gets the "current member" of the guild. This is the member that is currently logged in.
    /// </summary>
    /// <param name="api">The guild API to fetch members with.</param>
    /// <param name="userApi">The user API to fetch the current user with.</param>
    /// <param name="guildID">The ID of the guild to fetch the current member for.</param>
    /// <param name="ct">The cancellation token for this operation.</param>
    /// <returns></returns>
    public static async Task<Result<IGuildMember>> GetCurrentGuildMemberAsync(this IDiscordRestGuildAPI api, IDiscordRestUserAPI userApi, Snowflake guildID, CancellationToken ct = default)
    {
        var selfResult = await userApi.GetCurrentUserAsync(ct).ConfigureAwait(false);
        
        if (!selfResult.IsSuccess)
            return Result<IGuildMember>.FromError(selfResult.Error);
        
        var self = selfResult.Entity;
        
        var memberResult = await api.GetGuildMemberAsync(guildID, self.ID, ct).ConfigureAwait(false);
        
        if (!memberResult.IsSuccess)
            return Result<IGuildMember>.FromError(memberResult.Error);
        
        return Result<IGuildMember>.FromSuccess(memberResult.Entity);
    }

}