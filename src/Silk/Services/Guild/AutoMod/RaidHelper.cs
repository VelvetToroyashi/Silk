using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Remora.Rest.Core;
using Remora.Results;
using Silk.Services.Data;

namespace Silk.Services.Guild;

public class RaidHelper : BackgroundService
{
    private readonly GuildConfigCacheService _config;
    
    private record Raider(Snowflake GuildID, Snowflake RaiderID, string Reason);

    private readonly Channel<Raider> _raiders = Channel.CreateUnbounded<Raider>();
    
    private readonly ConcurrentDictionary<Snowflake, List<JoinRange>>        _joinRanges = new();
    private readonly ConcurrentDictionary<Snowflake, (DateTimeOffset LastJoin, int Count)> _joins = new();
    
    public RaidHelper(GuildConfigCacheService config)
    {
        _config = config;
    }

    public async Task<Result> HandleJoinAsync(Snowflake GuildID, Snowflake UserID, DateTimeOffset joined, bool hasAvatar)
    {
        TrimBuckets(GuildID);

        var config = await _config.GetConfigAsync(GuildID);
        
        if (!config.EnableRaidDetection)
            return Result.FromSuccess();

        var joinCounter = _joins.GetOrAdd(GuildID, _ => new());

        if (joinCounter.LastJoin - DateTimeOffset.UtcNow > TimeSpan.FromSeconds(config.RaidCooldownSeconds))
        {
            joinCounter.LastJoin = DateTimeOffset.UtcNow;
            joinCounter.Count = 0;
            
            _joins[GuildID] = joinCounter;
        }
        
        if (joinCounter.Count++ >= config.RaidDetectionThreshold)
        {
            await _raiders.Writer.WriteAsync(new Raider(GuildID, UserID, "Raid Detection: Join velocity check exceeds threshold."));
            
            joinCounter.LastJoin = DateTimeOffset.UtcNow;
            
            _joins[GuildID] = joinCounter;
            
            return Result.FromSuccess();
        }
        
        // Join velocity check failed. Check for chunked raid activity.
        var joinRanges = _joinRanges.GetOrAdd(GuildID, new List<JoinRange>());

        var range = joinRanges.FirstOrDefault(r => r.InRange(joined));

        if (range is null)
        {
            range = new(joined.AddHours(-2), joined.AddHours(.5), config.RaidDetectionThreshold);
            joinRanges.Add(range);
        }

        if (range.IsSuspect(joined))
        {
            await _raiders.Writer.WriteAsync(new Raider(GuildID, UserID, "Raid Detection: Join cluster check failed."));
            return Result.FromSuccess();
        }
        
        //TODO: Check for avatar-less users & calculate risk factor based on account date
        
        return Result.FromSuccess();
    }
    
    private void TrimBuckets(Snowflake GuildID)
    {
        if (!_joinRanges.TryGetValue(GuildID, out var ranges) || !_joins.TryGetValue(GuildID, out var joins))
            return;

        var expiration = DateTimeOffset.UtcNow.AddMinutes(-2);

        for (var i = ranges.Count - 1; i <= 0; i--)
        {
            var range = ranges[i];
            if (range.Added > expiration)
                continue;

            ranges.Remove(range);
        }
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken) => null;
}

public sealed class JoinRange
{
    private int _count = 1;
    
    private readonly int            _max;
    private readonly DateTimeOffset _from;
    private readonly DateTimeOffset _to;
    
    public DateTimeOffset Added { get; }
    
    public JoinRange(DateTimeOffset from, DateTimeOffset to, int max)
    {
        _from = from;
        _to = to;
        _max = max;
        
        Added = DateTimeOffset.UtcNow;
    }

    public bool InRange(DateTimeOffset join) => join >= _from && join <= _to;

    public bool IsSuspect(DateTimeOffset time)
    {
        var suspicious = InRange(time);
        
        if (suspicious)
        {
            _count++;
        }

        return _count > _max;
    }
}