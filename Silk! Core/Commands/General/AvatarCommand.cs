using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using SilkBot.Utilities;

namespace SilkBot.Commands.General
{
    [Category(Categories.General)]
    public class AvatarCommand : BaseCommandModule
    {
        [Command("Avatar")]
        [Description("Show your, or someone else's avatar!")]
        public async Task GetAvatarAsync(CommandContext ctx)
        {
            await ctx.RespondAsync(embed:
                new DiscordEmbedBuilder()
                .WithImageUrl(ctx.User.AvatarUrl.Replace("128", "4096"))
                .WithColor(DiscordColor.CornflowerBlue)
                .WithFooter("Silk", ctx.Client.CurrentUser.AvatarUrl)
                .WithTimestamp(DateTime.Now));

        }

        [Command("Avatar")]
        public async Task GetAvatarAsync(CommandContext ctx, [Description("Test pog?")] DiscordUser user)
        {
            await ctx.RespondAsync(embed:
                new DiscordEmbedBuilder()
                .WithAuthor(ctx.Member.DisplayName, iconUrl: ctx.Member.AvatarUrl)
                .WithDescription($"{user.Mention}'s Avatar")
                .WithImageUrl(user.AvatarUrl.Replace("128", "4096"))
                .WithColor(DiscordColor.CornflowerBlue)
                .WithFooter("Silk", ctx.Client.CurrentUser.AvatarUrl)
                .WithTimestamp(DateTime.Now));
        }


        [Command("Avatar")]
        public async Task GetAvatarAsync(CommandContext ctx, [RemainingText] string mention)
        {
            var user = ctx.Guild.Members.First(m => m.Value.DisplayName.ToLower().Contains(mention.ToLower())).Value;

            await ctx.RespondAsync(embed:
                new DiscordEmbedBuilder()
                .WithAuthor(ctx.Member.DisplayName, iconUrl: ctx.Member.AvatarUrl)
                .WithDescription($"{user.Mention}'s Avatar")
                .WithImageUrl(user.AvatarUrl.Replace("128", "4096"))
                .WithColor(DiscordColor.CornflowerBlue)
                .WithFooter("Silk", ctx.Client.CurrentUser.AvatarUrl)
                .WithTimestamp(DateTime.Now));
        }

    }
}
