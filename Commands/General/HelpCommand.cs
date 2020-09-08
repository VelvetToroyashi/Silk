using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace SilkBot.Commands.General
{
    public class HelpCommand : BaseCommandModule
    {
        [Command("Help")]
        public async Task HelpPlusHelp(CommandContext ctx)
        {
            await ctx.Member.SendMessageAsync("https://github.com/VelvetThePanda/SilkBot/wiki");
        }
    }

}
