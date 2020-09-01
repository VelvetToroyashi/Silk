using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace SilkBot
{
    public class HelpCommandModule : BaseCommandModule
    {
        [Command("Help")]
        public async Task HelpPlusHelp(CommandContext ctx, string commandName = null)
        {
            await ctx.Member.SendMessageAsync("https://github.com/VevletThePanda/SilkBot/Wiki");
        }
    }

}
