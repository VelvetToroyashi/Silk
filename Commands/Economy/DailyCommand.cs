using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;


namespace SilkBot.Commands.Economy
{
    public class DailyCommand : BaseCommandModule
    {
        [Command("Daily")]
        public async Task DailyMoney(CommandContext ctx)
        {
            //TODO: rewrite Daily command to work with database instead
        }
    }
}
