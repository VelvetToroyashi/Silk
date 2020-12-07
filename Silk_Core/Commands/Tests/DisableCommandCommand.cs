using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace SilkBot.Commands.Tests
{
    public class DisableCommandCommand : BaseCommandModule
    {

        [Command]
        public async Task Disable(CommandContext ctx, string command) => 
        CommandProcessorModule.DisableCommand(command, ctx.Guild.Id);
        
    }
}