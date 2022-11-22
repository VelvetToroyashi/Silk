using System.Linq;
using System.Threading.Tasks;
using Mediator;
using Microsoft.Extensions.Logging;
using Prometheus;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Rest.Core;
using Remora.Results;
using Silk.Data.Entities;
using Silk.Data.MediatR.Guilds;
using Silk.Services.Data;
using Silk.Utilities;

namespace Silk.Services.Bot;

/// <summary>
///     Evaluates configured exemptions to determine if a a user is exempt from automated actions.
/// </summary>
public class ExemptionEvaluationService
{
    private readonly IMediator _mediator;
    private readonly IDiscordRestGuildAPI    _guildApi;
    private readonly ILogger<ExemptionEvaluationService> _logger;

    public ExemptionEvaluationService
    (
        IMediator mediator,
        IDiscordRestGuildAPI guildApi,
        ILogger<ExemptionEvaluationService> logger
    )
    {
        _mediator = mediator;
        _guildApi = guildApi;
        _logger   = logger;
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
        _logger.LogTrace("Evaluating exemption for {Exemption} in {GuildID} for {UserID} in {ChannelID}", exemptionType, guildID, userID, channelID);
        
        var config = await _mediator.Send(new GetGuildConfig.Request(guildID));

        using (SilkMetric.EvaluationExemptionTime.WithLabels(exemptionType.ToString()).NewTimer()) 
        {
            if (!config.Exemptions.Any())
                return Result<bool>.FromSuccess(false); // No exemptions
            
            Result<IGuildMember> guildMemberResult = await _guildApi.GetGuildMemberAsync(guildID, userID);

            if (!guildMemberResult.IsSuccess)
            {
                _logger.LogWarning("Failed to get guild member {UserID} in {GuildID}", userID, guildID);
                return Result<bool>.FromError(guildMemberResult.Error);
            }

            IGuildMember guildMember = guildMemberResult.Entity;

            foreach (ExemptionEntity exemption in config.Exemptions)
            {
                if (!exemption.Exemption.HasFlag(exemptionType))
                    continue;

                if (exemption.TargetType is ExemptionTarget.Channel)
                {
                    if (channelID == exemption.TargetID)
                    {
                        _logger.LogTrace("Found channel-level exemption for {Exemption} in {GuildID} for {UserID} in {ChannelID}", exemptionType, guildID, userID, channelID);
                        return Result<bool>.FromSuccess(true);
                    }
                }

                if (exemption.TargetType is ExemptionTarget.Role)
                {
                    if (guildMember.Roles.Any(r => r == exemption.TargetID))
                    {
                        _logger.LogTrace("Found role-level exemption for {Exemption} in {GuildID} for {UserID} in {ChannelID}", exemptionType, guildID, userID, channelID);
                        return Result<bool>.FromSuccess(true);
                    }
                }

                if (exemption.TargetType is ExemptionTarget.User)
                {
                    if (userID == exemption.TargetID)
                    {
                        _logger.LogTrace("Found user-level exemption for {Exemption} in {GuildID} for {UserID} in {ChannelID}", exemptionType, guildID, userID, channelID);
                        return Result<bool>.FromSuccess(true);
                    }
                }
            }

            return Result<bool>.FromSuccess(false);
        }
    }
}