using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Silk.Core.Utilities;
using Silk.Extensions;

namespace Silk.Core.Commands.General
{
    [Category(Categories.General)]
    public class Enbiggen : BaseCommandModule
    {
        [Command("enlarge")]
        [Description("Enlarge an emoji!")]
        public async Task Enlarge(CommandContext ctx, DiscordEmoji emoji)
        {
            // _ = emoji.Id == 0:
            //     ? await ctx.RespondAsync($"I can't enlarge unicode emojis, {ctx.User.Username}!")
            //     : await ctx.RespondAsync(embed: new DiscordEmbedBuilder()
            //         .WithDescription($"Emoji Name: {emoji.GetDiscordName()}")
            //         .WithImageUrl(emoji.Url)
            //         .WithColor(new DiscordColor("42d4f5"))
            //         .AddFooter(ctx));

            DiscordMessageBuilder builder = new();
            if (emoji.Id is 0)
            {
                builder.WithContent("I can't enlarge unicode emojis, yet.").WithReply(ctx.Message.Id);
                await ctx.RespondAsync(builder);
            }
            else
            {
                DiscordEmbed embed = new DiscordEmbedBuilder()
                    .WithDescription($"Emoji Name: {emoji.GetDiscordName()}")
                    .WithImageUrl(emoji.Url)
                    .WithColor(new DiscordColor("42d4f5"));
                
                builder.WithEmbed(embed).WithReply(ctx.Message.Id, true);
                await builder.SendAsync(ctx.Channel);
            }
        }
    }
}