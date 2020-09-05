using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System.Threading.Tasks;

namespace SilkBot
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
