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

    public async Task<IReadOnlyList<IUser>> GetSuspicousMembersAsync(Snowflake guildID, CancellationToken ct = default)
    {
        var db = _redis.GetDatabase();

        var lastCheck = (string?)await db.StringGetAsync($"Silk:SuspiciousMemberCheck:{guildID}");
        var time      = lastCheck is null ? DateTimeOffset.UtcNow : DateTimeOffset.Parse(lastCheck);
            
        if (DateTimeOffset.UtcNow > time + TimeSpan.FromHours(6))
        {
            // While we technically store members for 12 hours, we allow re-checking every 6 in case we've missed some gateway events
                
            _gateway.SubmitCommand(new RequestGuildMembers(guildID));

            var holder = 0; // Used instead of chunk.ChunkIndex >= ChunkCount because chunks arrive aysnchronously
            await _interactivity.WaitForEventAsync<IGuildMembersChunk>(gmc => gmc.GuildID == guildID && holder++ > gmc.ChunkCount, ct);

            await Task.CompletedTask;
        }
        
        await db.StringSetAsync($"Silk:SuspiciousMemberCheck:{guildID}", DateTimeOffset.UtcNow.ToString());

        // Unless 12h has magically passed, this will be here.
        var members = await _cache.TryGetValueAsync<IReadOnlyList<IGuildMember>>(KeyHelpers.CreateGuildMembersKey(guildID, default, default), ct);

        var query = members.Entity.Count > 5_000 ? members.Entity.AsParallel() : members.Entity.AsEnumerable();

        var phishing = query
                      .Where(u => _phishing.IsSuspectedPhishingUsername(u.User.Value.Username).IsSuspicious)
                      .Select(s => s.User.Value)
                      .ToArray();

        await _cache.CacheAsync($"Silk:SuspiciousMemberCheck:{guildID}:Members", phishing.Select(u => u.ID), ct);
        
        return phishing;
    }
}