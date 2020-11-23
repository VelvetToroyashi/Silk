using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using SilkBot.Extensions;
using SilkBot.Utilities;

namespace SilkBot.Commands.General
{
    [Category(Categories.General)]
    public class Enbiggen : BaseCommandModule
    {
        [Command("Enlarge")]
        public async Task Enlarge(CommandContext ctx, DiscordEmoji emoji)
        {
            _ = emoji.Id == 0
                ? await ctx.RespondAsync($"I can't enlarge unicode emojis, {ctx.User.Username}!")
                : await ctx.RespondAsync(embed: new DiscordEmbedBuilder()
                                                .WithDescription("Emoji Name: " + emoji.GetDiscordName())
                                                .WithImageUrl(emoji.Url)
                                                .WithColor(new DiscordColor("42d4f5"))
                                                .AddFooter(ctx));
        }
    }
}