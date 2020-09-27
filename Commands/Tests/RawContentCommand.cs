using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System.Linq;
using System.Threading.Tasks;

namespace SilkBot.Commands.Tests
{
    public class RawContentCommand : BaseCommandModule
    {
        [Command("Content"), RequireRoles(RoleCheckMode.Any, "SilkDev"), RequireGuild]
        public async Task Content(CommandContext ctx, DiscordMessage msg)
        {
            await ctx.RespondAsync(msg.Content, embed: msg.Embeds.FirstOrDefault());
        }
    }
}
