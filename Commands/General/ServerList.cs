using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System.Threading.Tasks;

namespace SilkBot
{
    public class ServerList : BaseCommandModule
    {
        [Command("Servers")]
        [Aliases("serverlist")]
        [HelpDescription("How many servers am I present on?")]
        public async Task Servers(CommandContext ctx) => await ctx.RespondAsync($"I am currently on {ctx.Client.Guilds.Count} servers!");

        // => -> -->
    }
}