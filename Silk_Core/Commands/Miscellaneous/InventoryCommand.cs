using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using SilkBot.Economy.Shop.Items;

namespace SilkBot.Commands.Miscellaneous
{
    public class InventoryCommand : BaseCommandModule
    {
        public static Dictionary<ulong, List<IBaseItem>> items = new();
        
        [Command]
        public async Task Inventory(CommandContext ctx)
        {
            DiscordEmbedBuilder embed = new DiscordEmbedBuilder().WithColor(DiscordColor.PhthaloGreen)
                                                                 .WithTitle($"{ctx.User.Username}'s Inventory")
                                                                 .WithDescription(
                                                                     "You have no items! Purchase some from the shop, or ask a friend to gift you one!");


            await ctx.RespondAsync(embed: embed);

            await Task.CompletedTask;
        }
    }
}