using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace SilkBot
{
    public class HelpCommandModule : BaseCommandModule
    {
        [HelpDescription("This very menu!")]
        [Command("Help")]

        public async Task HelpPlusHelp(CommandContext ctx, string commandName = null)
        {
            var key = (commandName ?? "help").ToLower();
            var eb = HelpCache.Entries[key];
            await ctx.Channel.SendMessageAsync(embed: eb);
        }
    }

}
