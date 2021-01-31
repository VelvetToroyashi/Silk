using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Silk.Core.Utilities;

namespace Silk.Core.Economy.Shop
{
    [Category(Categories.Economy)]
    [Experimental]
    [Hidden]
    [Group("shop")]
    public partial class ShopCommand : BaseCommandModule
    {
        //Basically the shop command will serve as a command processor.//
        [GroupCommand]
        public async Task Shop(CommandContext ctx)
        {
            await ctx.RespondAsync("Command usage:\n\t\t`shop [global | view <name> | create | delete]`");
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