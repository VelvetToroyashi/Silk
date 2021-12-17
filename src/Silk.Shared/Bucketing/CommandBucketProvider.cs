using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Remora.Rest.Core;
using Remora.Results;

namespace Silk.Shared.Types;

public class CommandBucketProvider : ICommandBucketProvider
{
    private const    string       BucketKey = "CommandBucket";
    private readonly IMemoryCache _cache;
    public CommandBucketProvider(IMemoryCache cache) => _cache = cache;


    public async ValueTask<Result<CommandBucket>> GetGlobalBucketAsync()                                                                                          => default;

    public async ValueTask<Result<CommandBucket>> GetScopedBucketAsync
    (
        Snowflake?        GuildID,
        Snowflake?        ChannelID,
        Snowflake?        UserID,
        CommandBucketType BucketType
    )
    {
        string bucketKey = BucketType is CommandBucketType.Global 
            ? $"{BucketKey}_Global"  
            : $"{BucketKey}_{GuildID}_{ChannelID}_{UserID}_{BucketType}";
        
        
        return default!;
    }
}