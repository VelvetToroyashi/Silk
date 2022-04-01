using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Remora.Discord.Gateway;
using Silk.Shared.Configuration;
using StackExchange.Redis;

namespace Silk.Services.Bot;

public class ShardAwareGateweayHelper : BackgroundService
{
    public int ShardID { get; private set; }

    private bool _setShardID;
    
    private static readonly TimeSpan ShardRefreshInterval = TimeSpan.FromSeconds(3);
    private static readonly TimeSpan ShardRefreshTimeout = TimeSpan.FromSeconds(5);
    
    private const string ShardPrefix         = "shard:";
    private const string ShardSessionPostfix = ":resume:session";
    private const string ShardSequencePostfix = ":resume:sequence";

    private Task? _shardRefreshTask;
    
    private readonly CancellationTokenSource _cts = new();
    
    private readonly DiscordGatewayClient              _client;
    private readonly IConnectionMultiplexer            _redis;
    private readonly SilkConfigurationOptions          _config;
    private readonly DiscordGatewayClientOptions       _options;
    private readonly ILogger<ShardAwareGateweayHelper> _logger;
    
    public ShardAwareGateweayHelper
    (
        DiscordGatewayClient                  client,
        IConnectionMultiplexer                redis,
        IOptions<SilkConfigurationOptions>     config,
        IOptions<DiscordGatewayClientOptions> options,
        ILogger<ShardAwareGateweayHelper>     logger
    )
    {
        _client  = client;
        _redis   = redis;
        _config  = config.Value;
        _options = options.Value;
        _logger  = logger;
    }

    public async ValueTask<int> GetAvailableShardIDAsync()
    {
        if (_setShardID)
            return ShardID;
        
        var redis = _redis.GetDatabase();
        var delay = 30;

        var found   = false;

        while (!_cts.Token.IsCancellationRequested)
        {
            int shardID;
            for (shardID = 0; shardID < _config.Discord.Shards; shardID++)
            {
                if (!redis.KeyExists($"{ShardPrefix}{ShardID}"))
                {
                    found       = true;
                    _setShardID = true;
                    ShardID     = shardID;
                    
                    _logger.LogInformation("Found available shard ID {ShardID}", shardID);

                    _shardRefreshTask = KeepAliveLoopAsync();
                    
                    break;
                }
            }

            if (found)
                break;

            _logger.LogInformation("No available shard IDs found, waiting {Delay} seconds", delay);
            
            await Task.Delay(TimeSpan.FromSeconds(delay), _cts.Token);
            
            delay = (int)(delay * 1.5);

        }

        return ShardID;
    }

    private async Task KeepAliveLoopAsync()
    {
        Debug.Assert(_setShardID);
        
        var redis = _redis.GetDatabase();

        var shardKey = $"{ShardPrefix}{ShardID}";
        
        while (!_cts.Token.IsCancellationRequested)
        {
            await redis.KeyExpireAsync(shardKey, ShardRefreshTimeout);
            
            await Task.Delay(ShardRefreshInterval, _cts.Token);
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Shard aware gateway helper started");

        await GetAvailableShardIDAsync();
        
        var resume = await LoadResumeDataAsync();

        if (resume.SessionID is not null)
        {
            var sessionField = typeof(DiscordGatewayClient).GetField("_sessionID", BindingFlags.Instance | BindingFlags.NonPublic);
            var sequenceField = typeof(DiscordGatewayClient).GetField("_lastSequenceNumber", BindingFlags.Instance | BindingFlags.NonPublic);
            
            sessionField!.SetValue(_client, resume.SessionID);
            sequenceField!.SetValue(_client, resume.Sequence);
        }

        try { await _client.RunAsync(stoppingToken); }
        catch { /* ignored */ }

        _cts.Cancel();
        await (_shardRefreshTask ?? Task.CompletedTask);
        
        await SaveResumeDataAsync();
        
        _logger.LogInformation("Shard aware gateway helper stopped");
    }
    
    private async Task<(string? SessionID, int Sequence)> LoadResumeDataAsync()
    {
        var redis = _redis.GetDatabase();
        
        var shardKey = $"{ShardPrefix}{ShardID}";
        
        var session = await redis.StringGetAsync(shardKey + ShardSessionPostfix);
        var sequence = await redis.StringGetAsync(shardKey + ShardSequencePostfix);
        
        if (session.IsNull || sequence.IsNull)
        {
            _logger.LogInformation("No resume data found for shard {ShardID}", ShardID);
            
            return (null, 0);
        }
        
        return (session, int.Parse(sequence));
    }

    private async Task SaveResumeDataAsync()
    {
        var redis = _redis.GetDatabase();
        
        var shardKey = $"{ShardPrefix}{ShardID}";
        
        var sessionID = typeof(DiscordGatewayClient).GetField("_sessionID", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(_client) as string;
        var sequence  = typeof(DiscordGatewayClient).GetField("_lastSequenceNumber", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(_client) as int?;
        
        if (sessionID is null || sequence is null)
            return;
        
        await redis.StringSetAsync(shardKey + ShardSessionPostfix, sessionID);
        await redis.StringSetAsync(shardKey + ShardSequencePostfix, sequence.ToString());
    }
}