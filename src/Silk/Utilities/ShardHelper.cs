using System;
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
    
    public bool IsRelevantToCurrentShard(Snowflake snowflake)
        => (int)snowflake.Value >> 22 % _shard.ShardCount == _shard.ShardID;
}