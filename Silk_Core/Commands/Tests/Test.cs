using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Silk_Items.Components;
using Silk_Items.Entities;
using SilkBot.Extensions;

namespace SilkBot.Commands.Tests
{
    [Group]
    public class Test : BaseCommandModule
    {
        private readonly Sword _sword = new();
        
        [Command]
        public async Task GetSword(CommandContext ctx) => 
            await ctx.RespondAsync($"Sword has: {_sword.Count()} components; " +
                                   $"`{_sword.Select(c => c.GetType().Name).JoinString(", ")}`");

        [Command]
        public async Task AddComponent(CommandContext ctx, string component = "")
        {
            
        }
        
    }
}