using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System.Threading.Tasks;

namespace SilkBot.Commands.Miscellaneous
{
    public class InventoryCommand : BaseCommandModule
    {



        [Command]
        public async Task Inventory(CommandContext ctx)
        {
            var embed = new DiscordEmbedBuilder().WithColor(DiscordColor.PhthaloGreen).WithTitle($"{ctx.User.Username}'s Inventory").WithDescription("You have no items! Purchase some from the shop, or ask a friend to gift you one!");



            await ctx.RespondAsync(embed: embed);

            await Task.CompletedTask;
        }
    }
}
