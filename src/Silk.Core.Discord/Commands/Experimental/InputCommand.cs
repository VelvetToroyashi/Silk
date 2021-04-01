using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Silk.Core.Discord.Services.Interfaces;

namespace Silk.Core.Discord.Commands.Experimental
{
    public class InputCommand : BaseCommandModule
    {
        private readonly IInputService _input;
        public InputCommand(IInputService input)
        {
            _input = input;
        }


        [Command]
        public async Task InputTest(CommandContext ctx)
        {
            await ctx.RespondAsync("Say anything!");
            string? result = await _input.GetStringInputAsync(ctx.User.Id, ctx.Channel.Id, ctx.Guild.Id);
            if (result is not null)
                await ctx.RespondAsync("Success!");
            else await ctx.RespondAsync("Darn. Try again!");
        }
    }
}