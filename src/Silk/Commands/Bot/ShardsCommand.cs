using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Gateway.Commands;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Contexts;
using Remora.Results;
using Silk.Shared.Constants;
using Silk.Utilities;
using StackExchange.Redis;
using Color = System.Drawing.Color;

namespace Silk.Commands.Bot;

public class ShardsCommand : CommandGroup
{
    private readonly ITextCommandContext         _context;
    private readonly IShardIdentification   _shard;
    private readonly IDiscordRestChannelAPI _channels;
    private readonly IConnectionMultiplexer _redis;
    
    public ShardsCommand
    (
        ITextCommandContext context,
        IShardIdentification shard,
        IDiscordRestChannelAPI channels,
        IConnectionMultiplexer redis
    )
    {
        _context = context;
        _shard = shard;
        _channels = channels;
        _redis = redis;
    }

    [Command("shards")]
    [Description("shows information about shards!")]
    public async Task<IResult> ShowShardInfoAsync()
    {
        var db         = _redis.GetDatabase();
        var shardCount = _shard.ShardCount;
        var fields     = new List<EmbedField>();

        for (int i = 0; i < Math.Min(shardCount, 25); i++)
        {
            var isUp = db.KeyExists(ShardHelper.GetShardIdentificationKey(i));
            
            var isCurrent = _shard.ShardID == i ? "(current)" : "";

            var sb = new StringBuilder();
            
            sb.AppendLine(isUp ? Emojis.ConfirmEmoji : Emojis.DeclineEmoji);

            sb.AppendLine("```cs");
            sb.AppendLine($"CPU:     {db.StringGet(ShardHelper.GetShardCPUUsageStatKey(i))}%");
            sb.AppendLine($"Memory:  {db.StringGet(ShardHelper.GetShardMemoryStatKey(i))} MB");
            sb.AppendLine($"Uptime:  {db.StringGet(ShardHelper.GetShardUptimeStatKey(i))}");
            sb.AppendLine($"Guilds:  {db.StringGet(ShardHelper.GetShardGuildCountStatKey(i))}");
            sb.AppendLine($"Members: {db.StringGet(ShardHelper.GetShardUserCountStatKey(i))}");
            sb.AppendLine("```");
            
            fields.Add(new($"Shard {i} {isCurrent}", sb.ToString(), true));
        }

        var embed = new Embed
        {
            Title = "All Shards",
            Fields = fields,
            Colour = Color.Goldenrod
        };
        
        return await _channels.CreateMessageAsync(_context.GetChannelID(), embeds: new[] { embed });
    }
}