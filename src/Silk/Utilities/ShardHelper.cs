using Remora.Discord.API.Abstractions.Gateway.Commands;
using Remora.Rest.Core;

namespace Silk.Utilities;

/// <summary>
/// A helper class that provides convenience methods regarding sharding, such as telling if a given guild ID is relevant to the current shard.
/// </summary>
public sealed class ShardHelper
{
    private readonly IShardIdentification _shard;
    public ShardHelper(IShardIdentification shard) => _shard = shard;
    
    public bool IsRelevantToCurrentShard(Snowflake? snowflake)
        => snowflake is {} sf ? (int)(sf.Value >> 22) % _shard.ShardCount == _shard.ShardID : _shard.ShardID is 0;
    
    public static string GetShardResumeSessionKey(int shardID)
        => $"shard:{shardID}:resume:session";
    
    public static string GetShardResumeSequenceKey(int shardID)
        => $"shard:{shardID}:resume:sequence";
    
    public static string GetShardIdentificationKey(int shardID)
        => $"shard:{shardID}";
    
    public static string GetShardGuildCountStatKey(int shardID)
        => $"shard:{shardID}:stat:guilds";
    
    public static string GetShardUserCountStatKey(int shardID)
        => $"shard:{shardID}:stat:users";
    
    public static string GetShardCPUUsageStatKey(int shardID)
        => $"shard:{shardID}:stat:cpu";
    
    public static string GetShardMemoryStatKey(int shardID)
        => $"shard:{shardID}:stat:memory";
    
    public static string GetShardLatencyStatKey(int shardID)
        => $"shard:{shardID}:stat:latency";
    
    public static string GetShardUptimeStatKey(int shardID)
        => $"shard:{shardID}:stat:uptime";

    public static string GetShardGuildsKey(int shardID)
        => $"shard:{shardID}:guilds";
}