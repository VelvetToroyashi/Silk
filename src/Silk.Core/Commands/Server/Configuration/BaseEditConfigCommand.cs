using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace Silk.Core.Commands.Server.Configuration
{
    public partial class BaseConfigCommand
    {
        [Group("edit")]
        public partial class BaseEditConfigCommand : BaseCommandModule
        {
            [GroupCommand]
            public async Task EditConfig(CommandContext ctx) =>
                await new DiscordMessageBuilder()
                    .WithReply(ctx.Message.Id, true)
                    .WithContent($"See `{ctx.Prefix}help config edit`.")
                    .SendAsync(ctx.Channel);
        }
    }
}