using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FuzzySharp;
using FuzzySharp.SimilarityRatio;
using FuzzySharp.SimilarityRatio.Scorer.Composite;
using Microsoft.Extensions.Logging;
using Prometheus;
using Remora.Discord.API;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Rest.Core;
using Remora.Results;
using Silk.Data.Entities;
using Silk.Services.Bot;
using Silk.Services.Data;
using Silk.Services.Interfaces;
using Silk.Shared.Constants;
using Silk.Utilities;
using Unidecode.NET;

namespace Silk.Services.Guild;

/// <summary>
///     Handles potential phishing links.
/// </summary>
public class PhishingDetectionService
{
    private record struct RavyAPIResponse(bool Matched, string? Key, double Similarity);
    
    private const           string Phishing  = "Message contained a phishing link.";
    private static readonly Regex  LinkRegex = new(@"[.]*(?:https?:\/\/(www\.)?)?(?<link>[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6})\b([-a-zA-Z0-9()@:%_\+.~#?&\/\/=]*)");


    private readonly HttpClient                        _http;
    private readonly IInfractionService                _infractions;
    private readonly IDiscordRestUserAPI               _users;
    private readonly IDiscordRestChannelAPI            _channels;
    private readonly PhishingGatewayService            _phishGateway;
    private readonly GuildConfigCacheService           _config;
    private readonly ExemptionEvaluationService        _exemptions;
    private readonly ILogger<PhishingDetectionService> _logger;

    public PhishingDetectionService
    (
        IHttpClientFactory                http,
        IInfractionService                infractions,
        IDiscordRestUserAPI               users,
        IDiscordRestChannelAPI            channels,
        PhishingGatewayService            phishGateway,
        GuildConfigCacheService           config,
        ExemptionEvaluationService        exemptions,
        ILogger<PhishingDetectionService> logger
    )
    {
        _http = http.CreateClient("ravy-api");
        _infractions  = infractions;
        _users        = users;
        _channels     = channels;
        _phishGateway = phishGateway;
        _config       = config;
        _exemptions   = exemptions;
        _logger       = logger;
    }

    private static readonly string[] SuspiciousUsernames = new[]
    {
        "Academy Moderator", "Bot Developer", "Bot Moderator", "Developer Message",
        "Discord Academy", "Discord Developers", "Discord Message", "Discord Moderation Academy",
        "Discord Moderator", "Discord Moderators", "Discord Staff", "Discord Terms", 
        "HypeSquad Academy", "Hypesquad Events", "Hypesquad Moderation Academy", "Hypesquad Moderation", 
        "Hypesquad Moderator", "ModMail", "Moderation Message", "Moderation Notification", 
        "Moderation Team", "Moderator Message", "Moderator Team", "Staff Academy",
        "Staff Developers", "Staff Events", "Staff Message", "Staff Moderators ", "System Message",
    };


    public async Task<Result> HandlePotentialSuspiciousAvatarAsync(Snowflake guildID, IUser user)
    {
        var now = DateTimeOffset.UtcNow;

        var config = await _config.GetModConfigAsync(guildID);
        
        if (!config.BanSuspiciousUsernames)
            return Result.FromSuccess();

        if (user.Avatar is null) // No avatar, no need to check.
            return Result.FromSuccess();
        
        var cdnResult = CDN.GetUserAvatarUrl(user.ID, user.Avatar);

        if (!cdnResult.IsSuccess)
            return Result.FromError(cdnResult.Error);
        
        var url = cdnResult.Entity;

        RavyAPIResponse response;

        using (SilkMetric.PhishingDetection.WithLabels("avatar").NewTimer())
        {
            response = await _http.GetFromJsonAsync<RavyAPIResponse>($"?avatar={url}&threshold=0.85");

            if (!response.Matched)
                return Result.FromSuccess();
        }

        _logger.LogDebug("Detected suspicious avatar in {TimeSpent:N0}ms", (DateTimeOffset.UtcNow - now).TotalMilliseconds);
        
        var selfResult = await _users.GetCurrentUserAsync();
        
        if (!selfResult.IsDefined(out var self))
            return Result.FromError(selfResult.Error!);
        
        var infractionResult = await _infractions.BanAsync
            (
             guildID,
             user.ID,
             self.ID,
             1,
             $"Potential Phishing UserBot; Matched Avatar: Similarity of {response.Similarity * 100}%",
             notify: false
            );
        
        return infractionResult.IsSuccess
            ? Result.FromSuccess()
            : Result.FromError(infractionResult.Error);
    }
    
    public async Task<Result> HandlePotentialSuspiciousUsernameAsync(Snowflake guildID, IUser user)
    {
        var config = await _config.GetModConfigAsync(guildID);

        if (!config.BanSuspiciousUsernames)
            return Result.FromSuccess();
        
        // TODO: add to config and make toggelable. This will go under phishing settings.
        var detection = IsSuspectedPhishingUsername(user.Username);
        
        if (!detection.isSuspicious)
            return Result.FromSuccess();

        var self = await _users.GetCurrentUserAsync();

        if (!self.IsSuccess)
            return Result.FromError(self.Error);

        // We delete the last day of messages to clear any potential join message.
        var infraction = await _infractions.BanAsync
            (
             guildID,
             user.ID,
             self.Entity.ID,
             1,
             $"Suspicious username similar to  '{detection.mostSimilarTo}' detected",
             notify: false
            );
        
        if (!infraction.IsSuccess)
            return Result.FromError(infraction.Error);
        
        return Result.FromSuccess();
    }
    
    private (bool isSuspicious, string mostSimilarTo) IsSuspectedPhishingUsername(string username)
    {
        using var _ = SilkMetric.PhishingDetection.WithLabels("username").NewTimer();
        
        var normalized = username.Unidecode();

        var fuzzy = Process.ExtractOne(normalized, SuspiciousUsernames, s => s, ScorerCache.Get<WeightedRatioScorer>());

        if (fuzzy.Score > 80)
            _logger.LogTrace("Potentially suspicious Username: {Normalized}, most similar to {FuzzyMatched}, Score: {Score}", normalized, fuzzy.Value, fuzzy.Score);
        
        // This is somewhat arbitrary, and may be adjusted to be more or less sensitive.
        return (fuzzy.Score > 95, fuzzy.Value);
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
            return Result.FromSuccess(); // DM channels are exempted.

        GuildModConfigEntity config = await _config.GetModConfigAsync(guildId);
        

        IEnumerable<string> links;

        using (SilkMetric.PhishingDetection.WithLabels("message").NewTimer())
        {
            links = LinkRegex.Matches(message.Content).Where(m => m.Success).Select(m => m.Groups["link"].Value).Where(_phishGateway.IsBlacklisted);
        }

        foreach (var match in links)
            SilkMetric.SeenPhishingLinks.WithLabels(match).Inc();
        
        if (!config.DetectPhishingLinks)
            return Result.FromSuccess(); // Phishing detection is disabled.
        
        if (links.Any())
        {
            var link = links.First();

            if (_phishGateway.IsBlacklisted(link))
            {
                _logger.LogInformation("Detected phishing link.");

                var exemptionResult = await _exemptions.EvaluateExemptionAsync(ExemptionCoverage.AntiPhishing, guildId, message.Author.ID, message.ChannelID);

                if (!exemptionResult.IsSuccess)
                    return Result.FromError(exemptionResult.Error);

                if (!exemptionResult.Entity)
                    return await HandleDetectedPhishingAsync(guildId, message.Author.ID, message.ChannelID, message.ID, config.DeletePhishingLinks);
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
            await _channels.DeleteMessageAsync(channelID, messageID);

        GuildModConfigEntity config = await _config.GetModConfigAsync(guildID);

        if (!config.NamedInfractionSteps.TryGetValue(AutoModConstants.PhishingLinkDetected, out InfractionStepEntity? step))
            return Result.FromError(new InvalidOperationError("Failed to get step for phishing link detected."));

        Result<IUser> selfResult = await _users.GetCurrentUserAsync();

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