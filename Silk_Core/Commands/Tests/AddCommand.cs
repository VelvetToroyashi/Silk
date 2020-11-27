using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace SilkBot.Commands.Tests
{
    public class AddCommand : BaseCommandModule
    {
        [Command]
        public async Task Add(CommandContext ctx, string item)
        {
            await ctx.RespondAsync("No.");
        }
        
    }
}