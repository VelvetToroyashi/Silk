#region

using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Silk.Core.Utilities;

#endregion

namespace Silk.Core.Commands.General
{
    [Category(Categories.General)]
    public class ServerList : BaseCommandModule
    {
        [Command("Servers")]
        [Aliases("serverlist")]
        [Description("How many servers am I present on?")]
        public async Task Servers(CommandContext ctx)
        {
            await ctx.RespondAsync($"I am currently on {ctx.Client.Guilds.Count} servers!");
        }

        // => -> --> 
    }
}