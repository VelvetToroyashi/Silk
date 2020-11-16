using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Threading.Tasks;
using SilkBot.Utilities;

namespace SilkBot.Commands.Miscellaneous
{
    [Category(Categories.Misc)]
    public class RepoCommand : BaseCommandModule
    {
        [Command("Repo")]
        public async Task Repository(CommandContext ctx)
        {
            var embed = new DiscordEmbedBuilder()
                .WithAuthor(ctx.Member.DisplayName, iconUrl: ctx.Member.AvatarUrl)
                .WithColor(DiscordColor.Blue)
                .WithTitle("Open Source!")
                .WithDescription("I'm an FOSS bot, so feel free to [look at the source code](https://github.com/VelvetThePanda/SilkBot), or if there's a bug or issue, [open an issue!](https://github.com/VelvetThePanda/SilkBot/issues).")
                .WithFooter("Silk!", ctx.Client.CurrentUser.AvatarUrl)
                .WithTimestamp(DateTime.Now);
            await ctx.RespondAsync(embed: embed);
        }

    }
}
