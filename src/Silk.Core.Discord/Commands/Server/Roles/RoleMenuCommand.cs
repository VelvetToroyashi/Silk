using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity.Extensions;

namespace Silk.Core.Discord.Commands.Server.Roles
{
    [Group]
    [RequireGuild]
    public class RoleMenuCommand : BaseCommandModule
    {
        [Command]
        public async Task Create(CommandContext ctx)
        {
            var interactivity = ctx.Client.GetInteractivity();
            var nameInputMessage = await ctx.RespondAsync("Alrighty, what would you like to name this rolemenu?");
            await interactivity.WaitForMessageAsync(m => m.Author == ctx.User && m.Channel == ctx.Message.Channel && !string.IsNullOrEmpty(m.Content));
        }
    }
}