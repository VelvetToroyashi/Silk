using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FuzzySharp;
using FuzzySharp.SimilarityRatio;
using FuzzySharp.SimilarityRatio.Scorer.Composite;
using Mediator;
using Microsoft.Extensions.Logging;
using Prometheus;
using Remora.Discord.API;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Caching;
using Remora.Discord.Caching.Services;
using Remora.Rest.Core;
using Remora.Results;
using Silk.Data.Entities;
using Silk.Data.MediatR.Guilds;
using Silk.Services.Bot;
using Silk.Services.Data;
using Silk.Services.Interfaces;
using Silk.Shared.Constants;
using Silk.Shared.Types;
using Silk.Utilities;
using StackExchange.Redis;
using Unidecode.NET;

namespace Silk.Services.Guild;

/// <summary>
///     Handles potential phishing links.
/// </summary>
public class PhishingDetectionService
{
    private const string AntiPhishAPIUrl = "https://api.phish.gg/check?id=";
    
    private record struct RavyAPIResponse(bool Matched, string? Key, double Similarity);
    
    private const           string Phishing  = "Message contained a phishing link.";
    private static readonly Regex  LinkRegex = new(@"[.]*(?:https?:\/\/(www\.)?)?(?<link>[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6})\b([-a-zA-Z0-9()@:%_\+.~#?&\/\/=]*)");

    private static readonly Regex InviteRegex = new(@"discord\.gg\/(?<invite>[a-zA-Z0-9]+)", RegexOptions.Compiled);

    private readonly IMediator                         _mediator;
    private readonly CacheService                      _cache;
    private readonly HttpClient                        _http;
    private readonly HttpClient                        _httpUnauthorized;
    private readonly IInfractionService                _infractions;
    private readonly IDiscordRestUserAPI               _users;
    private readonly IConnectionMultiplexer            _redis;
    private readonly IDiscordRestChannelAPI            _channels;
    private readonly IDiscordRestInviteAPI             _invites;
    private readonly PhishingGatewayService            _phishGateway;
    private readonly ExemptionEvaluationService        _exemptions;
    private readonly ILogger<PhishingDetectionService> _logger;

    public PhishingDetectionService
    (
        IMediator                         mediator,
        CacheService                      cache,
        IHttpClientFactory                http,
        IInfractionService                infractions,
        IDiscordRestUserAPI               users,
        IConnectionMultiplexer            redis,
        IDiscordRestInviteAPI             invites,
        IDiscordRestChannelAPI            channels,
        PhishingGatewayService            phishGateway,
        ExemptionEvaluationService        exemptions,
        ILogger<PhishingDetectionService> logger
    )
    {
        _mediator         = mediator;
        _cache            = cache;
        _http             = http.CreateClient("ravy-api");
        _httpUnauthorized = http.CreateClient();
        _infractions      = infractions;
        _users            = users;
        _redis            = redis;
        _channels         = channels;
        _invites          = invites;
        _phishGateway     = phishGateway;
        _exemptions       = exemptions;
        _logger           = logger;
    }

    private static readonly string[] SuspiciousUsernames = 
    { 
        "Academy Moderator", "Bot Developer", "Bot Moderator",
        "CapchaBot", "Developer Message", "Discord Academy", "Discord Bug Hunter", "Discord Developers", "Discord Message",
        "Discord Moderation Academy", "Discord Moderator", "Discord Moderators", "Discord Staff", "Discord Terms", "EventsHS",
        "Hype Email", "Hype Form Ads", "Hype Mail", "HypeSquad Academy", "Hypesquad Events", "Hypesquad Moderation",
        "Hypesquad Moderation Academy", "Hypesquad Moderator", "Join Hype", 
        "Mod Email", "Moderation Message", "Moderation Notification",
        "Moderation Team", "Moderator Message", "Moderator Team", "ModMail", "Recruitment Moderator",
        "Staff Academy", "Staff Developers", "Staff Events", "Staff Message", "Staff Moderators ", "System Message"
    };
    
    public async Task<Result> HandlePotentialSuspiciousAvatarAsync(Snowflake guildID, IUser user)
    {
        if (user.IsBot.IsDefined(out var bot) && bot)
            return Result.FromSuccess();
        
        var now = DateTimeOffset.UtcNow;

        var config = await _mediator.Send(new GetGuildConfig.Request(guildID));
        
        if (!config.BanSuspiciousUsernames)
            return Result.FromSuccess();

        if (user.Avatar is null) // No avatar, no need to check.
            return Result.FromSuccess();
        
        var priorUserResult = await _cache.TryGetPreviousValueAsync<IUser>(new KeyHelpers.UserCacheKey(user.ID));

        if (priorUserResult.IsDefined(out var priorUser) && !user.Avatar.Value.Equals(priorUser.Avatar?.Value)) // If the avatar hasn't changed, we don't care about this update.
            return Result.FromSuccess();
        
        var cdnResult = CDN.GetUserAvatarUrl(user.ID, user.Avatar);

        if (!cdnResult.IsSuccess)
            return (Result)cdnResult;
        
        var url = cdnResult.Entity;

        RavyAPIResponse response;

        using (SilkMetric.PhishingDetection.WithLabels("avatar").NewTimer())
        {
            response = await _http.GetFromJsonAsync<RavyAPIResponse>($"?avatar={url}&threshold=0.90");

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
         $"ANTI-PHISH: Suspicious avatar detected. \nCategory: `{response.Key}` \nSimilarity: {response.Similarity * 100}%",
         notify: false
        );
        
       return (Result)infractionResult;
    }
    
    public async Task<Result> HandlePotentialSuspiciousUsernameAsync(Snowflake guildID, IUser user, bool bypass)
    {
        if (!bypass)
        {
            var userBefore = await _cache.TryGetPreviousValueAsync<IUser>(new KeyHelpers.UserCacheKey(user.ID));
        
            if (!userBefore.IsDefined(out var priorUser) ||  user.Username == priorUser.Username)
                return Result.FromSuccess();
        }
        
        var config = await _mediator.Send(new GetGuildConfig.Request(guildID));

        if (!config.BanSuspiciousUsernames)
            return Result.FromSuccess();

        // TODO: add to config and make toggelable. This will go under phishing settings.
        var detection = IsSuspectedPhishingUsername(user.Username);

        if (!detection.IsSuspicious)
            return Result.FromSuccess();

        var self = await _users.GetCurrentUserAsync();

        if (!self.IsSuccess)
            return (Result)self;

        // We delete the last day of messages to clear any potential join message.
        var infraction = await _infractions.BanAsync
        (
         guildID,
         user.ID,
         self.Entity.ID,
         1,
         $"ANTI-PHISH: Detected suspicious username similar to `{detection.MostSimilarTo}`.",
         notify: false
        );

        return (Result)infraction;

    }
    
    public static (bool IsSuspicious, string MostSimilarTo) IsSuspectedPhishingUsername(string username)
    {
        using var _ = SilkMetric.PhishingDetection.WithLabels("username").NewTimer();
        
        var normalized = username.Unidecode();
        
        var fuzzy = Process.ExtractOne(normalized, SuspiciousUsernames, s => s, ScorerCache.Get<WeightedRatioScorer>());

        // This is somewhat arbitrary, and may be adjusted to be more or less sensitive.
        return (fuzzy.Score > 95, fuzzy.Value);
    }

    /// <summary>
    ///     Detects any phishing links in a given message.
    /// </summary>
    /// <param name="message">The message to scan.</param>
    public async Task<Result> DetectPhishingAsync(IMessageCreate message)
    {
        if (message.Author.IsBot.IsDefined(out bool bot) && bot)
            return Result.FromSuccess(); // Sus.

        if (!message.GuildID.IsDefined(out Snowflake guildID))
            return Result.FromSuccess(); // DM channels are exempted.

        var config = await _mediator.Send(new GetGuildConfig.Request(guildID));

        IEnumerable<string> links;

        using (SilkMetric.PhishingDetection.WithLabels("message").NewTimer())
        {
            links = LinkRegex.Matches(message.Content).Where(m => m.Success).Select(m => m.Groups["link"].Value);
        }

        var knownPhishingLinks = links.Where(_phishGateway.IsBlacklisted).ToArray();

        foreach (var match in knownPhishingLinks)
            SilkMetric.SeenPhishingLinks.WithLabels(match).Inc();
        
        if (!config.DetectPhishingLinks)
            return Result.FromSuccess(); // Phishing detection is disabled.
        
        foreach (var link in knownPhishingLinks)
        {
            if (!_phishGateway.IsBlacklisted(link))
                continue;
            
            _logger.LogInformation("Detected phishing link.");

            var exemptionResult = await _exemptions.EvaluateExemptionAsync(ExemptionCoverage.AntiPhishing, guildID, message.Author.ID, message.ChannelID);

            if (!exemptionResult.IsSuccess)
                return (Result)exemptionResult;

            if (!exemptionResult.Entity)
                return await HandleDetectedPhishingAsync(guildID, message.Author.ID, message.ChannelID, message.ID, config.DeletePhishingLinks);
        }
        
        // There aren't any cached phishing links in the message, but there's still invites.
        // Handle accordingly.

        var db = _redis.GetDatabase();

        foreach (var link in links.Except(knownPhishingLinks))
        {
            if (!InviteRegex.IsMatch(link))
                continue; // Previously we threw all links at the API and dealt with what stuck,
                          // but that's both wasteful, a privacy risk, and polutes our metrics
            
            var cacheResult = (bool?)await db.StringGetAsync(SilkKeyHelper.GenerateInviteKey(link));

            if (cacheResult is not null)
                continue;

            // It'd be nice to have a way to poke this endpoint with a HEAD request.
            var exists = await _invites.GetInviteAsync(link);

            if (!exists.IsDefined(out var serverRes) || !serverRes.Guild.IsDefined(out var guild))
                return Result.FromSuccess(); // We've 404'd, invite's dead.

            var apiResult = await _httpUnauthorized.GetAsync(AntiPhishAPIUrl + guild.ID);

            if (!apiResult.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to check invite: {Link}; is the API down?", link);
                continue;
            }
                
            var apiResponse = await JsonDocument.ParseAsync(await apiResult.Content.ReadAsStreamAsync());

            if (!apiResponse.RootElement.TryGetProperty("match", out var matchProp))
                continue; // There was an error, so forget it.
                
            var matched = matchProp.GetBoolean();
                
            var type = apiResponse.RootElement.TryGetProperty("reason", out var typeProp) ? $" ({typeProp})" : null;

            await db.StringSetAsync(SilkKeyHelper.GenerateInviteKey(link), matched);
                
            if (matched)
                return await HandleDetectedPhishingAsync(guildID, message.Author.ID, message.ChannelID, message.ID, true, $"Detected malicious invite {type}.");
        }
        
        return Result.FromSuccess();
    }

    /// <summary>
    ///     Handles a detected phishing link.
    /// </summary>
    /// <param name="guildID">The ID of the guild the message was detected on.</param>
    /// <param name="authorID">The ID of the author.</param>
    /// <param name="channelID">The ID of the channel.</param>
    /// <param name="messageID">The ID of the message.</param>
    /// <param name="delete">Whether to delete the detected phishing link.</param>
    private async Task<Result> HandleDetectedPhishingAsync(Snowflake guildID, Snowflake authorID, Snowflake channelID, Snowflake messageID, bool delete, string? reason = null)
    {
        if (delete)
            await _channels.DeleteMessageAsync(channelID, messageID);

        reason ??= Phishing;

        var config = await _mediator.Send(new GetGuildConfig.Request(guildID));

        if (!config.NamedInfractionSteps.TryGetValue(AutoModConstants.PhishingLinkDetected, out InfractionStepEntity? step))
            return Result.FromError(new InvalidOperationError("Failed to get step for phishing link detected."));

        Result<IUser> selfResult = await _users.GetCurrentUserAsync();

        if (!selfResult.IsSuccess)
            return (Result)selfResult;

        IUser self = selfResult.Entity;

        var infractionResult = step.Type switch
        {
            InfractionType.Ban    => await _infractions.BanAsync(guildID, authorID, self.ID, 0, reason),
            InfractionType.Kick   => await _infractions.KickAsync(guildID, authorID, self.ID, reason),
            InfractionType.Strike => await _infractions.StrikeAsync(guildID, authorID, self.ID, reason),
            InfractionType.Mute   => await _infractions.MuteAsync(guildID, authorID, self.ID, reason, step.Duration == TimeSpan.Zero ? null : step.Duration),
            _                     => throw new InvalidOperationException("Invalid infraction type.")
        };

        return (Result)infractionResult;
    }
}