using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;

namespace Silk.Core.Utilities
{
    public static class ThrowHelper
    {
        public static async Task MuteRoleNotFoundInDatabase(DiscordChannel channel) => _ = await channel.SendMessageAsync("Mute role isn't set up!");
        public static async Task MuteRoleNotFoundInGuild(DiscordChannel channel) => _ = await channel.SendMessageAsync("Configured mute role doesn't exist on the server!");
        public static async Task EmptyArgument(CommandContext ctx) => await BotExceptionHelper.SendHelpAsync(ctx.Client, ctx.Command.Name, ctx);
    }
}