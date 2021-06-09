using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.SlashCommands;
using Silk.Extensions;
using Silk.Extensions.DSharpPlus;

namespace Silk.Core.SlashCommands.Attributes
{
    public class RequireCommonGuildAttribute : SlashCheckBaseAttribute
    {
        public async override Task<bool> ExecuteChecksAsync(InteractionContext ctx)
            => ctx.Services
                .Get<DiscordShardedClient>()!
                .GetMember(m => m == ctx.User) is not null;
    }
}