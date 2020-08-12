using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System.Threading.Tasks;

namespace SilkBot
{
    public class PingCommand : BaseCommandModule
    {
        [Command("Ping")]
        [HelpDescription("Returns my API response time.")]
        public async Task Ping(CommandContext ctx)
        {
            await ctx.RespondAsync($"I'm here! Response time: {ctx.Client.Ping} ms");
        }

    }
}
