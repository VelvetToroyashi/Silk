using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Silk.Core.Utilities.HelpFormatter;

namespace Silk.Core.Commands.General
{
    [Category(Categories.General)]
    public class ServerList : BaseCommandModule
    {
        [Command("servers")]
        [Aliases("serverlist")]
        [Description("How many servers am I present on?")]
        public async Task Servers(CommandContext ctx) => await ctx.RespondAsync($"I am currently on {GetGuildCount()} servers!");
        private static int GetGuildCount() => Core.Bot.Instance!.Client.ShardClients.Values.SelectMany(s => s.Guilds.Keys).Count();

    }
}