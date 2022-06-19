using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.Caching.Abstractions.Services;
using Remora.Discord.Rest.Extensions;
using Remora.Rest;
using Remora.Rest.Core;
using Remora.Results;

namespace Silk.Extensions.Remora;

public static class IRestHttpClientExtensions
{
    /// <summary>
    /// Queries the API to request all members from a given guild.
    /// </summary>
    /// <param name="client">An instance of <see cref="IRestHttpClient"/> to make API calls with.</param>
    /// <param name="cache">A cache provider to ensure ratelimits aren't exceeded.</param>
    /// <param name="guildID">The ID of the guild to query.</param>
    /// <returns>A result that may or not have succeeded contianing the returned guild members.</returns>
    /// <remarks>Given that this method uses a bare rest client, it is up to the consumer to handle any caching of the return result.</remarks>
    public static async Task<Result<IReadOnlyList<IGuildMember>>> GetGuildMembersAsync(this IRestHttpClient client, ICacheProvider cache, Snowflake guildID)
    {
        var last    = default(Snowflake);
        var members = new List<IGuildMember>(1000);
        
        while (true)
        {
            var result = await client.GetAsync<IReadOnlyList<IGuildMember>>
            (
             $"/guilds/{guildID}/members",
             b => b.WithRateLimitContext(cache)
                   .AddQueryParameter("limit", "1000")
                   .AddQueryParameter("after", last.ToString())
            );

            if (!result.IsDefined(out var receivedMembers))
                return Result<IReadOnlyList<IGuildMember>>.FromError(result.Error!);
            
            // We've hit the end of the members list.
            if (!members.Any() || members.Count < 1000)
                return Result<IReadOnlyList<IGuildMember>>.FromSuccess(members);
            
            members.AddRange(receivedMembers);

            last = members[^1].User.Value.ID;
        }
    }
}