using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace Silk.Core.Commands.Tests
{
    public class ReplyCommand : BaseCommandModule
    {
        [Command]
        [Hidden]
        public async Task Reply(CommandContext ctx)
        {
            var builder = new DiscordMessageBuilder();
            builder.WithContent("OwO");
            var msg = await ctx.RespondAsync(builder);
            builder.WithReply(ctx.Message.Id);
            await msg.ModifyAsync(builder);
        }
    }
}