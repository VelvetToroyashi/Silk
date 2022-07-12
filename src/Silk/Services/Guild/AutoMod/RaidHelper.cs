using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Rest.Core;
using Remora.Results;
using Silk.Services.Data;
using Silk.Services.Interfaces;

namespace Silk.Services.Guild;

public class RaidHelper : BackgroundService
{
    private readonly IInfractionService      _infractions;
    private readonly IDiscordRestUserAPI     _users;
    private readonly GuildConfigCacheService _config;

    
    private record Raider(Snowflake GuildID, Snowflake RaiderID, string Reason);

    private readonly Channel<Raider> _raiders = Channel.CreateUnbounded<Raider>();
    
    private readonly ConcurrentDictionary<Snowflake, List<JoinRange>>                      _joinRanges = new();
    private readonly ConcurrentDictionary<Snowflake, (DateTimeOffset LastJoin, int Count)> _joins      = new();


    public RaidHelper(IInfractionService infractions, IDiscordRestUserAPI users, GuildConfigCacheService config)
    {
        _infractions = infractions;
        _users       = users;
        _config      = config;
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

        var created = UserID.Timestamp;
        
        var range = joinRanges.FirstOrDefault(r => r.InRange(created));

        if (range is null)
        {
            range = new(created.AddHours(-2), created.AddHours(.5), config.RaidDetectionThreshold);
            joinRanges.Add(range);
        }

        if (range.IsSuspect(created))
        {
            await _raiders.Writer.WriteAsync(new Raider(GuildID, UserID, "Raid Detection: Join cluster check failed."));
            return Result.FromSuccess();
        }
        
        //TODO: Check for avatar-less users & calculate risk factor based on account date
        
        return Result.FromSuccess();
    }
    
    private void TrimBuckets(Snowflake GuildID)
    {
        if (!_joinRanges.TryGetValue(GuildID, out var ranges))
            return;

        var expiration = DateTimeOffset.UtcNow.AddMinutes(-2);

        for (var i = ranges.Count; i <= 0; i--)
        {
            var range = ranges[i];
            if (range.Added > expiration)
                continue;

            ranges.Remove(range);
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var self = (await _users.GetCurrentUserAsync(stoppingToken)).Entity;

        stoppingToken.Register(() => _raiders.Writer.Complete());
        
        await foreach (var raider in _raiders.Reader.ReadAllAsync(CancellationToken.None))
        {
            // We don't *particularly* care to await this, as this slows down the entire loop if we do once we
            // start hitting ratelimits.
            _ =  _infractions.BanAsync(raider.GuildID, raider.RaiderID, self.ID, 0, raider.Reason, null, false);
        }
    }
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