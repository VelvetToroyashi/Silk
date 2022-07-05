using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using Humanizer;
using Humanizer.Localisation;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Gateway.Commands;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Gateway;
using Remora.Results;
using Silk.Utilities;
using Silk.Utilities.HelpFormatter;
using StackExchange.Redis;

namespace Silk.Commands.Bot;

[Category(Categories.Bot)]
public class BotStatCommand : CommandGroup
{
    private readonly ICommandContext        _context;
    private readonly IConnectionMultiplexer _redis;
    private readonly IDiscordRestChannelAPI _channels;
    private readonly IShardIdentification   _shard;
    private readonly DiscordGatewayClient   _gateway;
    
    public BotStatCommand
    (
        ICommandContext context,
        IConnectionMultiplexer redis,
        IDiscordRestChannelAPI channels,
        IShardIdentification shard,
        DiscordGatewayClient gateway
    )
    {
        _context  = context;
        _redis    = redis;
        _channels = channels;
        _shard    = shard;
        _gateway  = gateway;
    }
    
    [Command("botstats", "bs", "botinfo")]
    [Description("Get the current stats for Silk")]
    public async Task<Result> GetBotStatsAsync()
    {
        using var process  = Process.GetCurrentProcess();

        var db = _redis.GetDatabase();

        var members = (int)await db.StringGetAsync(ShardHelper.GetShardUserCountStatKey(_shard.ShardID));
        var guilds  = (int)await db.StringGetAsync(ShardHelper.GetShardGuildCountStatKey(_shard.ShardID));

        var heapMemory = $"{GC.GetTotalMemory(true) / 1024 / 1024:n0} MB";
        
        var embed = new Embed()
        {
            Title  = $"Stats (Shard {_shard.ShardID + 1}):",
            Colour = Color.Gold,
            Fields = new EmbedField[]
            {
                new("Latency:", $"{_gateway.Latency.TotalMilliseconds:n0} ms", true),
                new("Guilds:", guilds.ToString(), true),
                new("Members:", members.ToString(), true),
                new("Memory:", heapMemory, true),
                new("Threads:", $"{ThreadPool.ThreadCount}", true),
                new("Uptime:", $"{DateTimeOffset.UtcNow.Subtract(process.StartTime).Humanize(2, minUnit: TimeUnit.Second, maxUnit: TimeUnit.Day)}", true)
            }
        };
        
        var res = await _channels.CreateMessageAsync(_context.ChannelID, embeds: new[] { embed });
        
       return (Result)res;
    }
}