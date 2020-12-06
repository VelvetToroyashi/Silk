using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using SilkBot.Utilities;

namespace SilkBot.Economy.Shop
{
    
    [Group("shop")]
    [Category(Categories.Economy)]
    public partial class ShopCommand : BaseCommandModule
    {
        [GroupCommand]
        public async Task Shop(CommandContext ctx)
        {
            //TODO: Show glaobal shop.
        }
    }

    public partial class ShopCommand
    {
        [Command]
        public async Task Global(CommandContext ctx)
        {
            await ctx.RespondAsync("Placeholder global shop goes here");
        }
    }

    public partial class ShopCommand
    {
        [Command]
        public async Task View(CommandContext ctx, string shopname)
        {
            await ctx.RespondAsync("Placeholder shop embed goes here");
        }
    }

    public partial class ShopCommand
    {
        [Command]
        public async Task Create(CommandContext ctx)
        {
            await ctx.RespondAsync("Placeholder creation modal goes here");
        }
    }

    public partial class ShopCommand
    {
        [Command]
        public async Task Delete(CommandContext ctx)
        {
            await ctx.RespondAsync("Placeholder deletion modal goes here");
        }
    }
}