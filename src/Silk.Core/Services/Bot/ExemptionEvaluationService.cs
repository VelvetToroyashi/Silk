using System.Linq;
using System.Threading.Tasks;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Rest.Core;
using Remora.Results;
using Silk.Core.Data.Entities;
using Silk.Core.Services.Data;

namespace Silk.Core.Services.Bot;

/// <summary>
///     Evaluates configured exemptions to determine if a a user is exempt from automated actions.
/// </summary>
public class ExemptionEvaluationService
{
    private readonly GuildConfigCacheService _config;
    private readonly IDiscordRestGuildAPI    _guildApi;
    public ExemptionEvaluationService(GuildConfigCacheService config, IDiscordRestGuildAPI guildApi)
    {
        _config   = config;
        _guildApi = guildApi;
    }

    /// <summary>
    ///     Evaluates if a user is exempt from an automated action.
    /// </summary>
    /// <param name="exemptionType">The type of exemption to check for.</param>
    /// <param name="guildID">The ID of the guild the exemption applies to.</param>
    /// <param name="userID">The ID of the user the exemption should apply to.</param>
    /// <param name="channelID">The ID of the channel the exemption may apply to.</param>
    public async Task<Result<bool>> EvaluateExemptionAsync(ExemptionCoverage exemptionType, Snowflake guildID, Snowflake userID, Snowflake channelID)
    {
        GuildModConfigEntity config = await _config.GetModConfigAsync(guildID);

        if (config.Exemptions.Any())
            return Result<bool>.FromSuccess(false); // No exemptions

        Result<IGuildMember> guildMemberResult = await _guildApi.GetGuildMemberAsync(guildID, userID);

        if (!guildMemberResult.IsSuccess)
            return Result<bool>.FromError(guildMemberResult.Error);

        IGuildMember guildMember = guildMemberResult.Entity;

        foreach (ExemptionEntity exemption in config.Exemptions)
        {
            if (!exemption.Exemption.HasFlag(exemptionType))
                continue;

            if (exemption.TargetType is ExemptionTarget.Channel)
                if (channelID == exemption.TargetID)
                    return Result<bool>.FromSuccess(true);

            if (exemption.TargetType is ExemptionTarget.Role)
                if (guildMember.Roles.Any(r => r == exemption.TargetID))
                    return Result<bool>.FromSuccess(true);

            if (exemption.TargetType is ExemptionTarget.User)
                if (userID == exemption.TargetID)
                    return Result<bool>.FromSuccess(true);
        }

        return Result<bool>.FromSuccess(false);
    }
}