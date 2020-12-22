#region

using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Silk.Core.Utilities;

#endregion

namespace Silk.Core.Commands.Tests
{
    [Expiremental]
    public class DisableCommandCommand : BaseCommandModule
    {

        [Command]
        public async Task Disable(CommandContext ctx, string command) => 
        CommandProcessorModule.DisableCommand(command, ctx.Guild.Id);
        
    }
}