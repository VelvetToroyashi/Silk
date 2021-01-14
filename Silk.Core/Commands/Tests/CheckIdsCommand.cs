using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace Silk.Core.Commands.Tests
{
    public class CheckIdsCommand
    {
        [Command]
        public async Task CheckId(CommandContext ctx, [RemainingText] DiscordRole[] Ids)
        {
            await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":m_ok:"));
        }
    }
}