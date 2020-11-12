using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System.Threading.Tasks;

namespace SilkBot.Commands.Economy
{

    public class DonateCommand : BaseCommandModule
    {

        [Command("Donate")]
        [Aliases("Gift")]
        public async Task Donate(CommandContext ctx, int amount, DiscordMember recipient)
        {
            //TODO: Do this later :kekw:
        }
    }
}
