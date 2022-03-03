using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Rest.Core;
using Remora.Results;
using Silk.Data.Entities;
using Silk.Services.Bot;
using Silk.Services.Data;
using Silk.Services.Interfaces;
using Silk.Shared.Constants;

namespace Silk.Services.Guild;

/// <summary>
///     Handles potential phishing links.
/// </summary>
public class PhishingDetectionService
{
    private const           string Phishing  = "Message contained a phishing link.";
    private static readonly Regex  LinkRegex = new(@"[.]*(?:https?:\/\/(www\.)?)?(?<link>[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6})\b([-a-zA-Z0-9()@:%_\+.~#?&\/\/=]*)");


    private readonly IInfractionService                _infractions;
    private readonly IDiscordRestUserAPI               _userApi;
    private readonly IDiscordRestChannelAPI            _channelApi;
    private readonly PhishingGatewayService            _phishGateway;
    private readonly GuildConfigCacheService           _configService;
    private readonly ExemptionEvaluationService        _exemptions;
    private readonly ILogger<PhishingDetectionService> _logger;

    public PhishingDetectionService
    (
        IInfractionService                infractions,
        IDiscordRestUserAPI               userApi,
        IDiscordRestChannelAPI            channelApi,
        PhishingGatewayService            phishGateway,
        GuildConfigCacheService           configService,
        ExemptionEvaluationService        exemptions,
        ILogger<PhishingDetectionService> logger
    )
    {
        _infractions   = infractions;
        _userApi       = userApi;
        _channelApi    = channelApi;
        _phishGateway  = phishGateway;
        _configService = configService;
        _exemptions    = exemptions;
        _logger        = logger;
    }

    /// <summary>
    ///     Detects any phishing links in a given message.
    /// </summary>
    /// <param name="message">The message to scan.</param>
    public async Task<Result> DetectPhishingAsync(IMessage message)
    {
        if (message.Author.IsBot.IsDefined(out bool bot) && bot)
            return Result.FromSuccess(); // Sus.

        if (!message.GuildID.IsDefined(out Snowflake guildId))
            return Result.FromSuccess(); // DM channels are exmepted.

        GuildModConfigEntity config = await _configService.GetModConfigAsync(guildId);

        if (!config.DetectPhishingLinks)
            return Result.FromSuccess(); // Phishing detection is disabled.

        // As to why I don't use Regex.Match() instead:
        // Regex.Match casts its return value to a non-nullable Match.
        // Run(), the method which it invokes returns Match?, which can cause an unexpected null ref.
        // You'd think this would be documented, but I digress.
        // Source: https://source.dot.net/#System.Text.RegularExpressions/System/Text/RegularExpressions/Regex.cs,388
        MatchCollection links = LinkRegex.Matches(message.Content);

        foreach (Match match in links)
        {
            if (match is null)
                continue;

            if (match.Success)
            {
                string link = match.Groups["link"].Value;

                if (_phishGateway.IsBlacklisted(link))
                {
                    _logger.LogInformation("Detected phishing link.");

                    Result<bool> exemptionResult = await _exemptions.EvaluateExemptionAsync(ExemptionCoverage.Phishing, guildId, message.Author.ID, message.ChannelID);

                    if (!exemptionResult.IsSuccess)
                        return Result.FromError(exemptionResult.Error);

                    if (!exemptionResult.Entity)
                        return await HandleDetectedPhishingAsync(guildId, message.Author.ID, message.ChannelID, message.ID, config.DeletePhishingLinks);
                }
            }
        }

        return Result.FromSuccess();
    }

    /// <summary>
    ///     Handles a detected phishing link.
    /// </summary>
    /// <param name="guildID">The ID of the guild the message was detected on.</param>
    /// <param name="authorId">The ID of the author.</param>
    /// <param name="delete">Whether to delete the detected phishing link.</param>
    private async Task<Result> HandleDetectedPhishingAsync(Snowflake guildID, Snowflake authorID, Snowflake channelID, Snowflake messageID, bool delete)
    {
        if (delete)
            await _channelApi.DeleteMessageAsync(channelID, messageID);

        GuildModConfigEntity config = await _configService.GetModConfigAsync(guildID);

        if (!config.NamedInfractionSteps.TryGetValue(AutoModConstants.PhishingLinkDetected, out InfractionStepEntity? step))
            return Result.FromError(new InvalidOperationError("Failed to get step for phishing link detected."));

        Result<IUser> selfResult = await _userApi.GetCurrentUserAsync();

        if (!selfResult.IsSuccess)
            return Result.FromError(selfResult.Error);

        IUser self = selfResult.Entity;

        var infractionResult = step.Type switch
        {
            InfractionType.Ban    => await _infractions.BanAsync(guildID, authorID, self.ID, 0, Phishing),
            InfractionType.Kick   => await _infractions.KickAsync(guildID, authorID, self.ID, Phishing),
            InfractionType.Strike => await _infractions.StrikeAsync(guildID, authorID, self.ID, Phishing),
            InfractionType.Mute   => await _infractions.MuteAsync(guildID, authorID, self.ID, Phishing, step.Duration == TimeSpan.Zero ? null : step.Duration),
            _                     => throw new InvalidOperationException("Invalid infraction type.")
        };

        return infractionResult.IsSuccess 
            ? Result.FromSuccess() 
            : Result.FromError(infractionResult.Error);
    }

}