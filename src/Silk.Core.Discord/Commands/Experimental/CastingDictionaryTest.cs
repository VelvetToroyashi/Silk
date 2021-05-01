using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Silk.Shared.Abstractions.DSharpPlus.Concrete;
using Silk.Shared.Abstractions.DSharpPlus.Interfaces;

namespace Silk.Core.Discord.Commands.Experimental
{
    public class CastingDictionaryTest : BaseCommandModule
    {
        private readonly IMessageSender _sender;
        public CastingDictionaryTest(IMessageSender sender) => _sender = sender;

        [Command]
        public Task Dict(CommandContext ctx) => Dict(new CommandExecutionContext(ctx, _sender));
        private async Task Dict(ICommandExecutionContext ctx)
        {
            await Task.Delay(0);
        }
    }
}