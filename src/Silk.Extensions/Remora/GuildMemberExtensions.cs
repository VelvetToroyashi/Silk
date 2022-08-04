using System.Linq;
using System.Threading.Tasks;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Rest.Core;
using Remora.Results;

namespace Silk.Extensions.Remora;

public static class GuildMemberExtensions
{
    /// <summary>
    /// Determines whether a member has a given permission, taking administration roles into account.
    /// </summary>
    /// <param name="member">The member to compute the permissions for.</param>
    /// <param name="guildApi">An instance of an <see cref="IDiscordRestGuildAPI"/> to fetch the guild's roles from.</param>
    /// <param name="guildID">The ID of the guild to fetch the roles from.</param>
    /// <param name="permission">The permission to check.</param>
    /// <returns></returns>
    public static async Task<Result<bool>> HasPermissionAsync(this IGuildMember member, IDiscordRestGuildAPI guildApi, Snowflake guildID, DiscordPermission permission)
    {
        if (!member.User.IsDefined(out var user))
            return Result<bool>.FromError(new InvalidOperationError("User is not defined."));
        
        var guildRolesResult = await guildApi.GetGuildRolesAsync(guildID);
        
        if (!guildRolesResult.IsSuccess)
            return Result<bool>.FromError(guildRolesResult.Error);

        var guildRoles = guildRolesResult.Entity;

        var memberRoles = guildRoles
                         .Where(r => member.Roles.Contains(r.ID))
                         .ToArray();

        var permissions = DiscordPermissionSet.ComputePermissions(user.ID, guildRoles.First(), memberRoles);
        
        return Result<bool>.FromSuccess(permissions.HasPermission(DiscordPermission.Administrator) || permissions.HasPermission(permission));
    }
}