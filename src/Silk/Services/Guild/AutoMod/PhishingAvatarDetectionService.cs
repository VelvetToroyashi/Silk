using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Remora.Discord.API;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Rest.Core;
using Remora.Results;
using Silk.Services.Data;
using Silk.Services.Interfaces;

namespace Silk.Services.Guild;

/// <summary>
/// Service for detecting suspicious avatars via Ravy's API (https://api.ravy.org/)
/// </summary>
public class PhishingAvatarDetectionService
{
    private record struct RavyAPIResponse(bool Matched, string? Key, double Similarity);
    
    private readonly HttpClient                              _client;
    private readonly IInfractionService                      _infractions;
    private readonly IDiscordRestUserAPI                     _users;
    private readonly GuildConfigCacheService                 _config;
    private readonly ILogger<PhishingAvatarDetectionService> _logger;
    
    public PhishingAvatarDetectionService
    (
        IHttpClientFactory httpFactory,
        IInfractionService infractions,
        IDiscordRestUserAPI users,
        GuildConfigCacheService config,
        ILogger<PhishingAvatarDetectionService> logger
    )
    {
        _client      = httpFactory.CreateClient("ravy-api"); 
        _infractions = infractions;
        _users       = users;
        _config      = config;
        _logger      = logger;
    }

    /// <summary>
    /// Checks a member's avatar
    /// </summary>
    /// <param name="member">The member who's avatar to check.</param>
    public Task<Result> CheckAvatarAsync(IGuildMemberAdd member)
    {
        if (!member.User.IsDefined(out var user))
            return Task.FromResult(Result.FromSuccess());
        
        if (user.Avatar is not {} avatar || member.User.Value.IsBot.IsDefined(out var bot) && bot)
            return Task.FromResult(Result.FromSuccess());
        
        return CheckAvatarAsync(member.GuildID, member.User.Value.ID, avatar);
    }

    public Task<Result> CheckAvatarAsync(IGuildMemberUpdate member)
    {
        if (member.User.Avatar is not {} avatar || member.User.IsBot.IsDefined(out var bot) && bot)
            return Task.FromResult(Result.FromSuccess());

        return CheckAvatarAsync(member.GuildID, member.User.ID, avatar);
    }

    private async Task<Result> CheckAvatarAsync(Snowflake guildID, Snowflake userID, IImageHash avatarHash)
    {
        var now = DateTimeOffset.UtcNow;

        var config = await _config.GetModConfigAsync(guildID);
        
        if (!config.BanSuspiciousUsernames)
            return Result.FromSuccess();
        
        var cdnResult = CDN.GetUserAvatarUrl(userID, avatarHash);

        if (!cdnResult.IsSuccess)
            return Result.FromError(cdnResult.Error);
        
        var url = cdnResult.Entity;
        
        var response = await _client.GetFromJsonAsync<RavyAPIResponse>($"?avatar={url}&threshold=0.85");

        if (!response.Matched)
            return Result.FromSuccess();
        
        _logger.LogDebug("Detected suspicious avatar in {TimeSpent:N0}ms", (DateTimeOffset.UtcNow - now).TotalMilliseconds);
        
        var selfResult = await _users.GetCurrentUserAsync();
        
        if (!selfResult.IsDefined(out var self))
            return Result.FromError(selfResult.Error!);
        
        var infractionResult = await _infractions.BanAsync
        (
         guildID,
         userID,
         self.ID,
         1,
         $"Potential Phishing UserBot; Matched Avatar: Similarity of {response.Similarity * 100}%",
         notify: false
        );
        
        return infractionResult.IsSuccess
            ? Result.FromSuccess()
            : Result.FromError(infractionResult.Error);
    }
}