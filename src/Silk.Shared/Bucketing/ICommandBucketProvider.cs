using System.Threading.Tasks;
using Remora.Rest.Core;
using Remora.Results;

namespace Silk.Shared.Types;

public interface ICommandBucketProvider
{
    public ValueTask<Result<CommandBucket>> GetGlobalBucketAsync();

    public ValueTask<Result<CommandBucket>> GetScopedBucketAsync(Snowflake? GuildID, Snowflake? ChannelID, Snowflake? UserID, CommandBucketType BucketType);
}