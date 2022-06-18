using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Gateway.Commands;
using Remora.Discord.Caching;
using Remora.Discord.Caching.Services;
using Remora.Discord.Gateway;
using Remora.Rest.Core;
using Remora.Results;
using Silk.Extensions;
using Silk.Interactivity;
using StackExchange.Redis;

namespace Silk.Services.Guild;


public class MemberScannerService
{
    private readonly CacheService             _cache;
    private readonly InteractivityWaiter      _interactivity;
    private readonly DiscordGatewayClient     _gateway;
    private readonly IConnectionMultiplexer   _redis;
    private readonly PhishingDetectionService _phishing;

    
    public MemberScannerService
    (
        CacheService             cache,
        InteractivityWaiter      interactivity,
        DiscordGatewayClient     gateway,
        IConnectionMultiplexer   redis,
        PhishingDetectionService phishing
    )
    {
        _cache         = cache;
        _interactivity = interactivity;
        _gateway       = gateway;
        _redis         = redis;
        _phishing      = phishing;
    }


    public async Task<Result> ValidateCooldownAsync(Snowflake guildID)
    {
        var db = _redis.GetDatabase();

        var lastCheck = (string?)await db.StringGetAsync($"Silk:SuspiciousMemberCheck:{guildID}");
        var time      = lastCheck is null ? DateTimeOffset.UtcNow.AddHours(7) : DateTimeOffset.Parse(lastCheck);

        var delta = DateTimeOffset.UtcNow - time;

        if (delta < TimeSpan.FromHours(6))
            return Result.FromError(new InvalidOperationError($"Member scanning is only available every 6 hours. Check back {(DateTimeOffset.UtcNow + (TimeSpan.FromHours(6) - delta)).ToTimestamp()}!"));
        
        return Result.FromSuccess();
    }
    
    public async Task<Result<IReadOnlyList<IUser>>> GetSuspicousMembersAsync(Snowflake guildID, CancellationToken ct = default)
    {
        var db      = _redis.GetDatabase();
        var members = new List<IUser>();
        
        await db.StringSetAsync($"Silk:SuspiciousMemberCheck:{guildID}", DateTimeOffset.UtcNow.ToString());
        
        var nonce = $"{guildID}-{Random.Shared.NextInt64(long.MaxValue)}";
        
        _gateway.SubmitCommand(new RequestGuildMembers(guildID, nonce: nonce));
        
        var holder = 0; // Used instead of chunk.ChunkIndex >= ChunkCount because chunks arrive aysnchronously
        await _interactivity.WaitForEventAsync<IGuildMembersChunk>(gmc =>
        {
            if (!gmc.Nonce.IsDefined(out var eventNonce) || eventNonce != nonce)
                return false;
            
            members.AddRange(gmc.Members.Select(m => m.User.Value));
            
            return ++holder >= gmc.ChunkCount;
        }, ct);
        
        var query = members.Count > 5_000 ? members.AsParallel() : members.AsEnumerable();

        var phishing = query
                      .Where(u => _phishing.IsSuspectedPhishingUsername(u.Username).IsSuspicious)
                      .ToArray();

        await _cache.CacheAsync<IReadOnlyList<Snowflake>>($"Silk:SuspiciousMemberCheck:{guildID}:Members", phishing.Select(u => u.ID).ToArray(), ct);
        
        return phishing;
    }
}