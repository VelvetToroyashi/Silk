using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace Silk.Core.Commands.Tests
{
    public class ReplyCommand : BaseCommandModule
    {
        [Command]
        public async Task Reply(CommandContext ctx)
        {
            var builder = new DiscordMessageBuilder();
            builder.WithContent("This is with a reply!");
            builder.WithReply(ctx.Message.Id, true);
            await builder.SendAsync(ctx.Channel);

            
            
            builder.WithContent("This is without a reply!");
            builder.WithReply(0);
            await builder.SendAsync(ctx.Channel);
        }
    }
}