using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Silk.Core.Discord.Utilities.HelpFormatter;

namespace Silk.Core.Discord.Commands.General
{
    [Category(Categories.General)]
    public class InviteCommand : BaseCommandModule
    {
        [Command("invite")]
        [Description("Gives you the Oauth2 code to invite me to your server!")]
        public async Task Invite(CommandContext ctx)
        {
            var Oauth2 = $"https://discord.com/api/oauth2/authorize?client_id={ctx.Client.CurrentUser.Id}&permissions=502656214&scope=bot%20applications.commands";
            await ctx.RespondAsync(new DiscordEmbedBuilder()
                .WithColor(DiscordColor.Gold)
                .WithDescription($"You can invite me with [this Oauth2]({Oauth2}) Link!"));
        }
    }
}