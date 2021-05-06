using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Silk.Shared.Abstractions.DSharpPlus.Concrete;
using Silk.Shared.Abstractions.DSharpPlus.Interfaces;

namespace Silk.Core.Logic.Commands.Experimental
{
    public class CastingDictionaryTest : BaseCommandModule
    {
        private readonly IMessageSender _sender;
        public CastingDictionaryTest(IMessageSender sender) => _sender = sender;

        [Command]
        public Task Dict(CommandContext ctx) => Dict(new CommandExecutionContext(ctx, _sender));
        private async Task Dict(ICommandExecutionContext ctx)
        {
            var foo = ctx.Channel.Guild!;

            var guilds = new IGuild[1_000_000];
            for (int i = 0; i < 1_000_000; i++)
            {
                foo = foo.Channels[744881658809024532].Guild!;
                guilds[i] = foo;
            }

            await Task.Delay(0);
        }
    }
}