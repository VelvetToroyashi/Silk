using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Remora.Discord.API.Abstractions.Gateway.Commands;
using Remora.Discord.Gateway;
using StackExchange.Redis;

namespace Silk.Services.Bot;

public class ShardStatService : BackgroundService
{
    private readonly Process              _process;
    private readonly IShardIdentification _shard;
    private readonly IConnectionMultiplexer _redis;
    

    public ShardStatService(IOptions<DiscordGatewayClientOptions> options, IConnectionMultiplexer redis)
    {
        _process = Process.GetCurrentProcess();
        _shard   = options.Value.ShardIdentification!;
        _redis   = redis;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(15000, stoppingToken);
                
                var db = _redis.GetDatabase();

                var cpuKey = $"shard:{_shard.ShardID}:stats:cpu";
                var memKey = $"shard:{_shard.ShardID}:stats:mem";
                
                var cpu = _process.TotalProcessorTime.TotalMilliseconds;
                var mem = _process.WorkingSet64 / 1024 / 1024;
                
                await db.StringSetAsync(cpuKey, cpu);
                await db.StringSetAsync(memKey, mem);
            }
            catch (OperationCanceledException)
            {
                /* ignored */
            }
        }
    }

    private int GetCPUUsage()
    {
        var cpuOld = _process.TotalProcessorTime;
        
        _process.Refresh();
        
        var cpuNew = _process.TotalProcessorTime;
        
        var cpuDelta = (cpuNew - cpuOld).TotalMilliseconds;

        var usage = cpuDelta / 15000;
        
        return (int) usage;
    }
    
}