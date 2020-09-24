using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System.Threading.Tasks;

namespace SilkBot.Commands.Economy.Shop
{
    public class ShopCommand : BaseCommandModule
    {
        //Basically the shop command will serve as a command processor.//
        [Command("Shop")]
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task Shop(CommandContext ctx, string action)

        {

        }

    }
}
