using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Silk.Core.Discord;
using Silk.Core.Discord.Utilities.HelpFormatter;

namespace Silk.Core.Logic.Commands.General
{
    [Category(Categories.Bot)]
    public class ServerList : BaseCommandModule
    {
        private readonly Main _main;
        public ServerList(Main main) => _main = main;

        [Command("servers")]
        [Aliases("serverlist")]
        [Description("How many servers am I present on?")]
        public async Task Servers(CommandContext ctx)
        {
            int servers = _main.ShardClient.ShardClients.Values.SelectMany(s => s.Guilds.Keys).Count();

            await ctx.RespondAsync($"I am currently on {servers} servers!");
        }
    }
}