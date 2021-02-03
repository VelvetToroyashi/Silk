using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Silk.Core.Utilities;

namespace Silk.Core.Commands.Miscellaneous
{
    [Category(Categories.Misc)]
    public class RepoCommand : BaseCommandModule
    {
        [Command("repo")]
        [Description("Get the link to Silk's source code")]
        public async Task Repository(CommandContext ctx)
        {
            DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                .WithAuthor(ctx.Member.DisplayName, iconUrl: ctx.Member.AvatarUrl)
                .WithColor(DiscordColor.Blue)
                .WithTitle("Open Source!")
                .WithDescription("I'm an FOSS bot, so feel free to [look at the source code](https://github.com/VelvetThePanda/Silk), " +
                                 "or if there's a bug or issue, [open an issue!](https://github.com/VelvetThePanda/Silk/issues).")
                .WithFooter($"Silk! | Requested by: {ctx.User.Id}", ctx.User.AvatarUrl);
            await ctx.RespondAsync(embed);
        }
    }
}