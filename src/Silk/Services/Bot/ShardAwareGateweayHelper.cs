using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Remora.Discord.API.Abstractions.Gateway.Commands;
using Remora.Discord.Gateway;
using StackExchange.Redis;

namespace Silk.Services.Bot;

public class ShardAwareGateweayHelper : BackgroundService
{
    private static readonly TimeSpan ShardRefreshInterval = TimeSpan.FromSeconds(3);
    private static readonly TimeSpan ShardRefreshTimeout = TimeSpan.FromSeconds(5);
    
    private const string ShardPrefix         = "shard:";
    private const string ShardSessionPostfix = ":resume:session";
    private const string ShardSequencePostfix = ":resume:sequence";

    private Task? _shardRefreshTask;
    
    private readonly CancellationTokenSource _cts = new();

    private readonly IShardIdentification              _shard;
    private readonly DiscordGatewayClient              _client;
    private readonly IConnectionMultiplexer            _redis;
    private readonly IHostApplicationLifetime          _lifetime;
    private readonly ILogger<ShardAwareGateweayHelper> _logger;
    
    public ShardAwareGateweayHelper
    (
        IShardIdentification              shard,
        DiscordGatewayClient              client,
        IConnectionMultiplexer            redis,
        IHostApplicationLifetime          lifetime,
        ILogger<ShardAwareGateweayHelper> logger
    )
    {
        _shard  = shard;
        _client = client;
        _redis  = redis;
        _lifetime = lifetime;
        _logger = logger;
    }
    
    private async Task KeepAliveLoopAsync()
    {
        var redis = _redis.GetDatabase();

        var shardKey = $"{ShardPrefix}{_shard.ShardID}";
        
        while (!_cts.Token.IsCancellationRequested)
        {
            await redis.KeyExpireAsync(shardKey, ShardRefreshTimeout);
            
            await Task.Delay(ShardRefreshInterval, _cts.Token);
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Shard aware gateway helper started");

        _ = Task.Run(KeepAliveLoopAsync, CancellationToken.None);
        
        var resume = await LoadResumeDataAsync();

        if (resume.SessionID is not null)
        {
            var sessionField = typeof(DiscordGatewayClient).GetField("_sessionID", BindingFlags.Instance | BindingFlags.NonPublic);
            var sequenceField = typeof(DiscordGatewayClient).GetField("_lastSequenceNumber", BindingFlags.Instance | BindingFlags.NonPublic);
            var resumableField = typeof(DiscordGatewayClient).GetField("_isSessionResumable", BindingFlags.Instance | BindingFlags.NonPublic);
            
            sessionField!.SetValue(_client, resume.SessionID);
            sequenceField!.SetValue(_client, resume.Sequence);
            resumableField!.SetValue(_client, true);
        }

        try
        {
            var res = await _client.RunAsync(stoppingToken);

            if (!res.IsSuccess)
            {
                _logger.LogError("Shard aware gateway helper failed with error {@Error}", res.Error);
            }
        }
        catch { /* ignored */ }

        await SaveResumeDataAsync();
        
        _cts.Cancel();
        await (_shardRefreshTask ?? Task.CompletedTask);

        _logger.LogInformation("Shard aware gateway helper stopped");
    }
    
    private async Task<(string? SessionID, int Sequence)> LoadResumeDataAsync()
    {
        var redis = _redis.GetDatabase();
        
        var shardKey = $"{ShardPrefix}{_shard.ShardID}";
        
        var session = await redis.StringGetAsync(shardKey + ShardSessionPostfix);
        var sequence = await redis.StringGetAsync(shardKey + ShardSequencePostfix);
        
        if ((string?)session is null || (int)sequence is 0)
        {
            _logger.LogInformation("No resume data found for shard {ShardID}", _shard.ShardID);
            
            return (null, 0);
        }
        
        return (session, int.Parse(sequence!));
    }

    private async Task SaveResumeDataAsync()
    {
        var redis = _redis.GetDatabase();
        
        var shardKey = $"{ShardPrefix}{_shard.ShardID}";
        
        var sessionID = (string?)typeof(DiscordGatewayClient).GetField("_sessionID", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(_client);
        var sequence  = (int)typeof(DiscordGatewayClient).GetField("_lastSequenceNumber", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(_client)!;
        
        await redis.StringSetAsync(shardKey + ShardSessionPostfix, sessionID ?? "");
        await redis.StringSetAsync(shardKey + ShardSequencePostfix, sequence.ToString());

        _logger.LogInformation("Saved resume data for shard {ShardID}", _shard.ShardID);
    }
}